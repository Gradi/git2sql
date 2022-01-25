INSERT INTO "Commits"
    (
        "Id",
        "Message",
        "Encoding",
        "AuthorName",
        "AuthorEmail",
        "AuthorDateTime",
        "CommitterName",
        "CommitterEmail",
        "CommitterDateTime"
    )
VALUES
    (
        $id,
        $message,
        $encoding,
        $authorName,
        $authorEmail,
        $authorDateTime,
        $committerName,
        $committerEmail,
        $committerDateTime
    )
ON CONFLICT DO NOTHING
