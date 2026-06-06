BEGIN;

-- Insert roles
INSERT INTO asp_net_roles (id, concurrency_stamp, name, normalized_name)
VALUES
  ('a1111111-1111-1111-1111-111111111111', gen_random_uuid()::text, 'Admin', 'ADMIN'),
  ('b2222222-2222-2222-2222-222222222222', gen_random_uuid()::text, 'User', 'USER');

-- Insert users
INSERT INTO asp_net_users
  (id, access_failed_count, concurrency_stamp, created_at, email, email_confirmed, lockout_enabled,
   lockout_end, normalized_email, normalized_user_name, password_hash, phone_number, phone_number_confirmed,
   profile_picture_url, security_stamp, two_factor_enabled, updated_at, user_name)
VALUES
  (
    '2b8f1a9c-1d3e-4f4d-9b2c-0a1b2c3d4e5f',
    0,
    gen_random_uuid()::text,
    now(),
    'alice@example.test',
    false,
    false,
    NULL,
    'ALICE@EXAMPLE.TEST',
    'ALICE',
    'AQAAAAEAACcQAAAAEexamplehashforalice==',
    NULL,
    false,
    NULL,
    gen_random_uuid()::text,
    false,
    now(),
    'alice'
  ),
  (
    '3c9f2b0d-2e4f-5a6b-8c3d-1b2c3d4e5f60',
    0,
    gen_random_uuid()::text,
    now(),
    'bob@example.test',
    false,
    false,
    NULL,
    'BOB@EXAMPLE.TEST',
    'BOB',
    'AQAAAAEAACcQAAAAEexamplehashforbob==',
    NULL,
    false,
    NULL,
    gen_random_uuid()::text,
    false,
    now(),
    'bob'
  );

-- Link users to roles
INSERT INTO asp_net_user_roles (user_id, role_id)
VALUES
  ('2b8f1a9c-1d3e-4f4d-9b2c-0a1b2c3d4e5f', 'a1111111-1111-1111-1111-111111111111'), -- alice -> Admin
  ('3c9f2b0d-2e4f-5a6b-8c3d-1b2c3d4e5f60', 'b2222222-2222-2222-2222-222222222222'); -- bob -> User

-- Insert refresh tokens (token must be unique)
INSERT INTO refresh_tokens (id, created_at, created_by_ip, expires, replaced_by_token, revoked, revoked_by_ip, token, user_id)
VALUES
  (1, now(), '127.0.0.1', now() + interval '7 days', NULL, NULL, NULL, 'refresh-token-alice-001', '2b8f1a9c-1d3e-4f4d-9b2c-0a1b2c3d4e5f'),
  (2, now(), '127.0.0.1', now() + interval '7 days', NULL, NULL, NULL, 'refresh-token-bob-001', '3c9f2b0d-2e4f-5a6b-8c3d-1b2c3d4e5f60');

-- Insert password reset tokens
INSERT INTO password_reset_tokens (id, expires, token_hash, user_id)
VALUES
  (1, now() + interval '1 day', 'pwd-reset-hash-1', '2b8f1a9c-1d3e-4f4d-9b2c-0a1b2c3d4e5f'),
  (2, now() + interval '1 day', 'pwd-reset-hash-2', '3c9f2b0d-2e4f-5a6b-8c3d-1b2c3d4e5f60');

-- Insert comments (EpisodeId and TitleId nullable)
INSERT INTO comments (id, episode_id, is_deleted, text, title_id, updated_at, user_id)
VALUES
  (1, NULL, FALSE, 'Sample comment from Alice (no title)', NULL, now(), '2b8f1a9c-1d3e-4f4d-9b2c-0a1b2c3d4e5f'),
  (2, NULL, FALSE, 'Sample comment from Bob (no title)', NULL, now(), '3c9f2b0d-2e4f-5a6b-8c3d-1b2c3d4e5f60');

-- Insert votes (title_id and episode_id nullable)
INSERT INTO votes (id, episode_id, title_id, updated_at, user_id, value)
VALUES
  (1, NULL, NULL, now(), '2b8f1a9c-1d3e-4f4d-9b2c-0a1b2c3d4e5f', 8),
  (2, NULL, NULL, now(), '3c9f2b0d-2e4f-5a6b-8c3d-1b2c3d4e5f60', 7);

-- Insert user content activities
INSERT INTO user_content_activities (id, activity_type, content_id, content_type, user_id, watched_at)
VALUES
  (1, 1, 1234, 1, '2b8f1a9c-1d3e-4f4d-9b2c-0a1b2c3d4e5f', now()),
  (2, 2, 5678, 2, '3c9f2b0d-2e4f-5a6b-8c3d-1b2c3d4e5f60', now());

-- Insert watch lists for users
INSERT INTO watch_lists (id, name, user_id)
VALUES
  (1, 'Alice''s Watchlist', '2b8f1a9c-1d3e-4f4d-9b2c-0a1b2c3d4e5f'),
  (2, 'Bob''s Favorites', '3c9f2b0d-2e4f-5a6b-8c3d-1b2c3d4e5f60');

-- Reset sequences for tables that use integer identity columns and were seeded above.
-- This uses pg_get_serial_sequence to find the sequence for the given table/column,
-- then sets it to MAX(id)+1 so the next nextval will yield a non-conflicting id.

SELECT setval(
    pg_get_serial_sequence('refresh_tokens', 'id'),
    COALESCE((SELECT MAX(id) FROM refresh_tokens), 0) + 1,
    false
);

SELECT setval(
    pg_get_serial_sequence('password_reset_tokens', 'id'),
    COALESCE((SELECT MAX(id) FROM password_reset_tokens), 0) + 1,
    false
);

SELECT setval(
    pg_get_serial_sequence('comments', 'id'),
    COALESCE((SELECT MAX(id) FROM comments), 0) + 1,
    false
);

SELECT setval(
    pg_get_serial_sequence('votes', 'id'),
    COALESCE((SELECT MAX(id) FROM votes), 0) + 1,
    false
);

SELECT setval(
    pg_get_serial_sequence('user_content_activities', 'id'),
    COALESCE((SELECT MAX(id) FROM user_content_activities), 0) + 1,
    false
);

SELECT setval(
    pg_get_serial_sequence('watch_lists', 'id'),
    COALESCE((SELECT MAX(id) FROM watch_lists), 0) + 1,
    false
);

COMMIT;

-- docker exec -t watchly-db pg_dump -U nk --clean --if-exists --format=p -d watchly > backup.sql
