BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "music_list" (
	"id"	BLOB NOT NULL,
	"name"	TEXT NOT NULL,
	"type"	INTEGER NOT NULL DEFAULT 0,
	"publish_date"	TEXT,
	"_owned_by"	TEXT,
	"_art"	TEXT,
	"is_deletable"	INTEGER NOT NULL DEFAULT 1,
	FOREIGN KEY("_art") REFERENCES "art"("address"),
	PRIMARY KEY("id")
);
INSERT INTO "music_list" VALUES ('11f830737ff4bc41a4ffe792d073f41f','Lieblingslieder',6,NULL,NULL,NULL,0);
COMMIT;
