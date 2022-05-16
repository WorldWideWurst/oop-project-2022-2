BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "artist" (
	"name"	TEXT NOT NULL,
	PRIMARY KEY("name")
);
CREATE TABLE IF NOT EXISTS "_music_in_list" (
	"_music"	BLOB NOT NULL,
	"_list"	BLOB NOT NULL,
	FOREIGN KEY("_list") REFERENCES "music_list"("id"),
	FOREIGN KEY("_music") REFERENCES "music"("id"),
	PRIMARY KEY("_music","_list")
);
CREATE TABLE IF NOT EXISTS "music_list" (
	"id"	BLOB NOT NULL,
	"name"	TEXT NOT NULL,
	"type"	TEXT,
	"publish_date"	TEXT,
	"_owned_by"	TEXT,
	PRIMARY KEY("id")
);
CREATE TABLE IF NOT EXISTS "source" (
	"address"	TEXT NOT NULL,
	"type"	TEXT NOT NULL DEFAULT 'audio',
	"_source_of"	BLOB NOT NULL,
	FOREIGN KEY("_source_of") REFERENCES "music"("id"),
	PRIMARY KEY("address")
);
CREATE TABLE IF NOT EXISTS "music" (
	"id"	BLOB NOT NULL,
	"title"	TEXT,
	"album" TEXT,
	PRIMARY KEY("id")
);
CREATE TABLE IF NOT EXISTS "_music_by_artist" (
	"_music"	BLOB NOT NULL,
	"_artist"	TEXT NOT NULL,
	FOREIGN KEY("_artist") REFERENCES "artist"("name"),
	FOREIGN KEY("_music") REFERENCES "music"("id"),
	PRIMARY KEY("_music","_artist")
);
COMMIT;
