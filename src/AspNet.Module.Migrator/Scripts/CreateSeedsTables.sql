BEGIN;

-- CREATE TABLE "__ef_seeds_history" --------------------------------
CREATE TABLE IF NOT EXISTS "public"."__ef_seeds_history"
(
    "seed_id"
    Text,
    "product_version"
    Text,
    PRIMARY
    KEY
(
    "seed_id"
)
    );

COMMIT;
