-- DEV ONLY: the original password hashes are unknown, so every seeded account
-- gets the password "123456" (legacy MD5 format) for the local demo database.
UPDATE "Account" SET "Password" = md5('123456');
