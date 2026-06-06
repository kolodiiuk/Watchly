BEGIN;

DELETE FROM episodes e
USING seasons s
WHERE e.season_id = s.id
  AND s.title_id IN (1374375, 1374382);

DELETE FROM seasons
WHERE title_id IN (1374375, 1374382);

WITH season_seed (title_id, ordinal_number, name, season_start, episode_count, runtime_minutes) AS (
    VALUES
        -- Game of Thrones
        (1374375, 1, 'Season 1', TIMESTAMPTZ '2011-04-17 00:00:00+00', 10, 57),
        (1374375, 2, 'Season 2', TIMESTAMPTZ '2012-04-01 00:00:00+00', 10, 57),
        (1374375, 3, 'Season 3', TIMESTAMPTZ '2013-03-31 00:00:00+00', 10, 57),
        (1374375, 4, 'Season 4', TIMESTAMPTZ '2014-04-06 00:00:00+00', 10, 57),
        (1374375, 5, 'Season 5', TIMESTAMPTZ '2015-04-12 00:00:00+00', 10, 57),
        (1374375, 6, 'Season 6', TIMESTAMPTZ '2016-04-24 00:00:00+00', 10, 57),
        (1374375, 7, 'Season 7', TIMESTAMPTZ '2017-07-16 00:00:00+00', 7, 59),
        (1374375, 8, 'Season 8', TIMESTAMPTZ '2019-04-14 00:00:00+00', 6, 72),

        -- Breaking Bad
        (1374382, 1, 'Season 1', TIMESTAMPTZ '2008-01-20 00:00:00+00', 7, 47),
        (1374382, 2, 'Season 2', TIMESTAMPTZ '2009-03-08 00:00:00+00', 13, 47),
        (1374382, 3, 'Season 3', TIMESTAMPTZ '2010-03-21 00:00:00+00', 13, 47),
        (1374382, 4, 'Season 4', TIMESTAMPTZ '2011-07-17 00:00:00+00', 13, 47),
        (1374382, 5, 'Season 5', TIMESTAMPTZ '2012-07-15 00:00:00+00', 16, 47)
),
inserted_seasons AS (
    INSERT INTO seasons (ordinal_number, name, title_id)
    SELECT
        ss.ordinal_number,
        ss.name,
        ss.title_id
    FROM season_seed ss
    RETURNING id, title_id, ordinal_number
)
INSERT INTO episodes (
    season_id,
    tv_show_id,
    ordinal_number,
    runtime,
    name,
    poster_url,
    release_date,
    updated_at
)
SELECT
    s.id AS season_id,
    s.title_id AS tv_show_id,
    episode_no AS ordinal_number,
    ss.runtime_minutes AS runtime,
    CONCAT('Episode ', episode_no) AS name,
    NULL AS poster_url,
    ss.season_start + ((episode_no - 1) * INTERVAL '7 days') AS release_date,
    NOW() AS updated_at
FROM inserted_seasons s
JOIN season_seed ss
  ON ss.title_id = s.title_id
 AND ss.ordinal_number = s.ordinal_number
CROSS JOIN LATERAL generate_series(1, ss.episode_count) AS episode_no;

COMMIT;
