-- 1. SEED LOOKUP TABLES
-- \copy genres (id, name) FROM '/tmp/normalized_output/GenreEntity.csv' WITH CSV HEADER NULL ''
-- \copy spoken_languages (id, name) FROM '/tmp/normalized_output/SpokenLanguageEntity.csv' WITH CSV HEADER NULL ''
-- \copy production_company (id, name) FROM '/tmp/normalized_output/ProductionCompaniesEntity.csv' WITH CSV HEADER NULL ''
-- \copy keywords (id, name) FROM '/tmp/normalized_output/KeywordEntity.csv' WITH CSV HEADER NULL ''

-- 2. SEED MAIN ENTITIES

-- Insert Movies (15 base columns)
-- \copy titles (id, release_date, updated_at, runtime, content_type, avg_tmdb_rating, is_adult, name, home_page, overview, poster_url, tagline, director, actors, localization_languages) FROM '/tmp/normalized_output/movies/TitleEntity.csv' WITH CSV HEADER NULL ''

-- Insert TV Shows (15 base columns + 12 TV columns)
\copy titles (id, release_date, updated_at, runtime, content_type, avg_tmdb_rating, is_adult, name, home_page, overview, poster_url, tagline, director, actors, localization_languages, number_of_seasons, number_of_episodes, original_language, vote_average, first_air_date, last_air_date, in_production, original_name, type, status, created_by, episode_run_time) FROM '/tmp/normalized_output/tv-shows/TvShowEntity.csv' WITH CSV HEADER NULL ''

-- 3. SEED JUNCTION TABLES
\copy title_genres (id, title_id, is_tv_show, genre_id) FROM '/tmp/normalized_output/GenreTitle.csv' WITH CSV HEADER NULL ''
\copy title_spoken_languages (id, title_id, is_tv_show, spoken_language_id) FROM '/tmp/normalized_output/SpokenLanguageTitle.csv' WITH CSV HEADER NULL ''
\copy title_production_companies (id, title_id, is_tv_show, production_company_id) FROM '/tmp/normalized_output/ProductionCompanyTitle.csv' WITH CSV HEADER NULL ''
\copy keyword_titles (id, title_id, is_tv_show, keyword_id) FROM '/tmp/normalized_output/KeywordTitle.csv' WITH CSV HEADER NULL ''

-- ==============================================================================
-- 4. RESET SEQUENCES (Optional but recommended)
-- ==============================================================================

-- 1. Lookup Tables
SELECT setval(pg_get_serial_sequence('genres', 'id'), COALESCE((SELECT MAX(id) FROM genres), 1));
SELECT setval(pg_get_serial_sequence('spoken_languages', 'id'), COALESCE((SELECT MAX(id) FROM spoken_languages), 1));
SELECT setval(pg_get_serial_sequence('production_company', 'id'), COALESCE((SELECT MAX(id) FROM production_company), 1));
SELECT setval(pg_get_serial_sequence('keywords', 'id'), COALESCE((SELECT MAX(id) FROM keywords), 1));

-- 2. Main Entities
SELECT setval(pg_get_serial_sequence('titles', 'id'), COALESCE((SELECT MAX(id) FROM titles), 1));
-- Uncomment the following line ONLY if the tv_shows table actually exists in the database
-- SELECT setval(pg_get_serial_sequence('tv_shows', 'id'), COALESCE((SELECT MAX(id) FROM tv_shows), 1));

-- 3. Junction Tables
SELECT setval(pg_get_serial_sequence('title_genres', 'id'), COALESCE((SELECT MAX(id) FROM title_genres), 1));
SELECT setval(pg_get_serial_sequence('title_spoken_languages', 'id'), COALESCE((SELECT MAX(id) FROM title_spoken_languages), 1));
SELECT setval(pg_get_serial_sequence('title_production_companies', 'id'), COALESCE((SELECT MAX(id) FROM title_production_companies), 1));
SELECT setval(pg_get_serial_sequence('keyword_titles', 'id'), COALESCE((SELECT MAX(id) FROM keyword_titles), 1));
