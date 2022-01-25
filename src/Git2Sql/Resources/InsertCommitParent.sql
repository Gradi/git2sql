INSERT INTO "CommitParents"
    (
        "Id",
        "ParentId"
    )
VALUES
    (
        $id,
        $parentId
    )
ON CONFLICT DO NOTHING
