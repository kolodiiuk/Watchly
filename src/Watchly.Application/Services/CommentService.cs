using Microsoft.EntityFrameworkCore;
using Npgsql;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Comments;
using Watchly.Application.Models.UserProfile;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Application.Services;

public sealed class CommentService : ICommentService
{
    private readonly WatchlyDbContext _dbContext;

    public CommentService(WatchlyDbContext context)
    {
        _dbContext = context;
    }

    public async Task<Result<IEnumerable<CommentDto>>> GetCommentsAsync(int id, bool isTitle, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            IEnumerable<CommentDto> comments;
            if (isTitle)
            {
                comments = await GetTitleCommentsAsync(id, ct).ToListAsync(ct);
            }
            else
            {
                comments = await GetEpisodeCommentsAsync(id, ct).ToListAsync(ct);
            }

            return Result<IEnumerable<CommentDto>>.Success(comments);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<IEnumerable<CommentDto>>.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<IEnumerable<CommentDto>>.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> LeaveCommentAsync(LeaveCommentRequest request, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
            {
                return Result.Fail("user is not found");
            }

            var comment = new Comment
            {
                EpisodeId = request.IsTitle ? null : request.ContentId,
                TitleId = request.IsTitle ? request.ContentId : null,
                Text = request.Text,
                IsDeleted = false,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId,
            };
            await _dbContext.AddAsync(comment, ct);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> UpdateCommentAsync(int commentId, string text, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
            {
                return Result.Fail("user is not found");
            }

            var comment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId, ct);
            if (comment is null)
            {
                return Result.Fail("comment is not found");
            }

            if (comment.UserId != userId)
            {
                return Result.Fail("user doesn't own comment");
            }

            comment.Text = text;
            comment.UpdatedAt = DateTime.UtcNow;
            _dbContext.Update(comment);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> DeleteCommentAsync(int commentId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
            {
                return Result.Fail("user is not found");
            }

            var comment = await _dbContext.Comments.FirstOrDefaultAsync(c => c.Id == commentId, ct);
            if (comment is null)
            {
                return Result.Fail("comment is not found");
            }

            if (comment.UserId != userId)
            {
                return Result.Fail("user doesn't own comment");
            }

            comment.IsDeleted = true;
            _dbContext.Update(comment);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    private IQueryable<CommentDto> GetEpisodeCommentsAsync(int id, CancellationToken ct)
    {
        var comments = _dbContext.Comments
            .Include(c => c.User)
            .Where(c => c.EpisodeId == id)
            .Where(c => !c.IsDeleted)
            .Select(c => new CommentDto(
                c.Id,
                c.EpisodeId.Value,
                false,
                c.UserId,
                c.UpdatedAt,
                c.Text,
                new UserDto
                {
                    Id = c.User.Id,
                    Email = c.User.Email,
                    UserName = c.User.UserName,
                    ProfilePicture = c.User.ProfilePictureUrl
                }));

        return comments;
    }

    private IQueryable<CommentDto> GetTitleCommentsAsync(int id, CancellationToken ct)
    {
        var comments = _dbContext.Comments
            .Include(c => c.User)
            .Where(c => c.TitleId == id)
            .Where(c => !c.IsDeleted)
            .Select(c => new CommentDto(
                c.Id,
                c.TitleId.Value,
                true,
                c.UserId,
                c.UpdatedAt,
                c.Text,
                new UserDto
                {
                    Id = c.User.Id,
                    Email = c.User.Email,
                    UserName = c.User.UserName,
                    ProfilePicture = c.User.ProfilePictureUrl
                }));

        return comments;
    }
}
