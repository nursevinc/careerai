using System;
using System.Collections.Generic;
using System.Text;
using CareerAI.Domain.Entities;
using CareerAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CareerAI.Infrastructure.Services;

// Skill tablosu ortak bir sözlüktür: "Docker" tek satır, herkes ona bağlanır.
public class SkillCatalog(AppDbContext db)
{
    // İstenen isimlerin hepsi için Skill kaydı döndürür; olmayanları oluşturur.
    // Anahtar: istenen isim (büyük-küçük harf duyarsız). Değer: sözlükteki kayıt (ilk eklenen yazım biçimi korunur).
    public async Task<Dictionary<string, Skill>> GetOrCreateAsync(
        IReadOnlyCollection<string> names, CancellationToken ct = default)
    {
        var result = new Dictionary<string, Skill>(StringComparer.OrdinalIgnoreCase);
        if (names.Count == 0) return result;

        var nameArray = names.ToArray();

        // SQL Server varsayılan collation'ı büyük-küçük harf duyarsız: "docker" == "Docker"
        var existing = await db.Skills.Where(s => nameArray.Contains(s.Name)).ToListAsync(ct);
        foreach (var skill in existing)
            result[skill.Name] = skill;

        var missing = nameArray.Where(name => !result.ContainsKey(name)).ToList();
        if (missing.Count == 0) return result;

        var created = missing.Select(name => new Skill { Name = name }).ToList();
        db.Skills.AddRange(created);

        try
        {
            await db.SaveChangesAsync(ct);
            foreach (var skill in created)
                result[skill.Name] = skill;
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // Başka bir istek aynı becerileri aynı anda eklemiş: bizimkileri bırakıp yeniden oku
            foreach (var skill in created)
                db.Entry(skill).State = EntityState.Detached;

            result.Clear();
            var reloaded = await db.Skills.Where(s => nameArray.Contains(s.Name)).ToListAsync(ct);
            foreach (var skill in reloaded)
                result[skill.Name] = skill;
        }

        return result;
    }
}