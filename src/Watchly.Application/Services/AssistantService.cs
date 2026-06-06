using OpenAI.Chat;
using System.Text.Json;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Comments;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Application.Services;

public sealed class AssistantService : IAssistantService
{
    private readonly WatchlyDbContext _dbContext;
    private readonly ICommentService _commentService;
    private readonly ChatClient _chatClient;

    public AssistantService(WatchlyDbContext context, ICommentService commentService, ChatClient chatClient)
    {
        _dbContext = context;
        _commentService = commentService;
        _chatClient = chatClient;
    }

    public async Task<Result<IEnumerable<CommentHighlightDto>>> GetRelevantCommentInfoAsync(int id, bool isTitle, RelevantCommentRequest request, Guid userId, CancellationToken ct)
    {
        try
        {
            IEnumerable<CommentDto> comments = _commentService.GetCommentsAsync(id, isTitle, ct).Result.Value;
            if(userId != Guid.Empty && request.ExcludeOwnComments)
            {
                comments = comments.Where(c => c.UserId != userId);
            }

            var commentsJson = JsonSerializer.Serialize(
                comments.Select(c => new
                {
                    c.Id,
                    c.Text
                }));

            var exampleJson = """
                  {
                    "Id": 1,
                    "Highlighted": "text of the <mark> relevant part </mark> comment"
                  }
                """;

            var prompt = $"""
                You are a text highlighting system.

                Highlight ONLY fragments where users are writing something relevant to the topic:
                "{request.Topic}". Highlight full phrases and sentences, not single words, include words 
                that are important for the context of what is being discussed.

                Wrap relevant fragments into HTML <mark> tags.

                Return ONLY valid JSON.

                Format:
                [
                  {exampleJson}
                ]

                Comments:
                {commentsJson}
                """;

            var response = await _chatClient.CompleteChatAsync(
                new List<ChatMessage> { (ChatMessage)prompt },
                cancellationToken: ct);

            var content = response.Value.Content[0].Text;

            var result = JsonSerializer.Deserialize<List<CommentHighlightDto>>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result is null
                ? Result<IEnumerable<CommentHighlightDto>>.Fail("Getting response from AI Assistant failed")
                : Result<IEnumerable<CommentHighlightDto>>.Success(result);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            return Result<IEnumerable<CommentHighlightDto>>.Fail(e.Message);
        }
    }
}
