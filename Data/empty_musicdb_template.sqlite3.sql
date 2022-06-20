BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "artist" (
	"name"	TEXT NOT NULL,
	PRIMARY KEY("name")
);
CREATE TABLE IF NOT EXISTS "_music_by_artist" (
	"_music"	BLOB NOT NULL,
	"_artist"	TEXT NOT NULL,
	FOREIGN KEY("_artist") REFERENCES "artist"("name"),
	FOREIGN KEY("_music") REFERENCES "music"("id"),
	PRIMARY KEY("_music","_artist")
);
CREATE TABLE IF NOT EXISTS "_music_in_list" (
	"_music"	BLOB NOT NULL,
	"_list"	BLOB NOT NULL,
	"position"	INTEGER,
	"date_added"	TEXT,
	FOREIGN KEY("_music") REFERENCES "music"("id"),
	FOREIGN KEY("_list") REFERENCES "music_list"("id"),
	PRIMARY KEY("_music","_list")
);
CREATE TABLE IF NOT EXISTS "music" (
	"id"	BLOB NOT NULL,
	"title"	TEXT,
	"album"	TEXT,
	"_album"	BLOB,
	"last_played"	TEXT,
	"first_registered"	TEXT NOT NULL,
	"_art"	TEXT,
	"duration"	REAL,
	"type"	TEXT,
	"play_count"	INTEGER NOT NULL DEFAULT 0,
	FOREIGN KEY("_art") REFERENCES "art"("address"),
	PRIMARY KEY("id")
);
CREATE TABLE IF NOT EXISTS "music_list" (
	"id"	BLOB NOT NULL,
	"name"	TEXT NOT NULL,
	"type"	TEXT,
	"publish_date"	TEXT,
	"_owned_by"	TEXT,
	"_art"	TEXT,
	"is_deletable"	INTEGER NOT NULL DEFAULT 1,
	FOREIGN KEY("_art") REFERENCES "art"("address"),
	PRIMARY KEY("id")
);
CREATE TABLE IF NOT EXISTS "art" (
	"address"	TEXT NOT NULL,
	"checksum"	INTEGER,
	PRIMARY KEY("address")
);
CREATE TABLE IF NOT EXISTS "source" (
	"address"	TEXT NOT NULL,
	"type"	TEXT NOT NULL DEFAULT 'local',
	"_source_of"	BLOB NOT NULL,
	"checksum"	INTEGER,
	FOREIGN KEY("_source_of") REFERENCES "music"("id"),
	PRIMARY KEY("address")
);
INSERT INTO "music_list" VALUES ('11f830737ff4bc41a4ffe792d073f41f','Lieblingslieder','playlist',NULL,NULL,NULL,0);
COMMIT;
