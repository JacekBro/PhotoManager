public async Task BuildDuplicateGroupsAsync(CancellationToken ct = default)
{
    var groups = await _db.Files
        .Where(x => x.HashSha256 != null)
        .GroupBy(x => x.HashSha256!)
        .Where(g => g.Count() > 1)
        .Select(g => new
        {
            Hash = g.Key,
            Files = g.ToList()
        })
        .ToListAsync(ct);

    foreach (var g in groups)
    {
        var group = await _db.DuplicateGroups.FirstOrDefaultAsync(x => x.GroupKey == g.Hash, ct);
        if (group == null)
        {
            group = new DuplicateGroup
            {
                GroupKey = g.Hash,
                CreatedUtc = DateTime.UtcNow
            };
            _db.DuplicateGroups.Add(group);
            await _db.SaveChangesAsync(ct);
        }

        var primary = g.Files.OrderByDescending(x => x.SizeBytes).ThenBy(x => x.FullPath).First();

        foreach (var file in g.Files)
        {
            var exists = await _db.DuplicateMembers.AnyAsync(x => x.DuplicateGroupId == group.Id && x.FileEntryId == file.Id, ct);
            if (exists) continue;

            _db.DuplicateMembers.Add(new DuplicateMember
            {
                DuplicateGroupId = group.Id,
                FileEntryId = file.Id,
                IsPrimary = file.Id == primary.Id,
                MatchScore = 1.0
            });
        }

        await _db.SaveChangesAsync(ct);
    }
}
