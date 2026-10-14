using System;
using System.Collections.Generic;
using System.Text;

using System.Text.RegularExpressions;

namespace CareerAI.Application.Services;

// Beceri adlarını karşılaştırılabilir bir anahtara çevirir.
// Kasıtlı olarak küçük ve okunabilir bir sözlük: gerçek verilerde eksik görürsek buraya ekleriz.
public static partial class SkillNormalizer
{
    // "(OOP)" gibi parantez içleri
    private static readonly Regex Parenthetical = new(@"\([^)]*\)", RegexOptions.Compiled);

    // Sondaki sürüm numarası: "asp.net core 8", "python 3.11", "c# 12"
    private static readonly Regex TrailingVersion = new(@"\s+v?\d+(\.\d+)*$", RegexOptions.Compiled);

    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.Compiled);

    // Takma ad -> standart anahtar (anahtarlar normalizasyondan SONRAKI hâlde yazılır: küçük harf, tire/eğik çizgi boşluk)
    private static readonly Dictionary<string, string> Aliases = new()
    {
        ["csharp"] = "c#",
        ["c sharp"] = "c#",

        ["dotnet"] = ".net",
        ["dotnet core"] = ".net",
        [".net core"] = ".net",
        [".net framework"] = ".net",

        ["aspnet core"] = "asp.net core",
        ["asp net core"] = "asp.net core",
        ["asp.net core web api"] = "asp.net core",

        ["ef core"] = "entity framework",
        ["efcore"] = "entity framework",
        ["entity framework core"] = "entity framework",

        ["mssql"] = "sql server",
        ["ms sql"] = "sql server",
        ["mssql server"] = "sql server",
        ["ms sql server"] = "sql server",
        ["microsoft sql server"] = "sql server",

        ["js"] = "javascript",
        ["java script"] = "javascript",
        ["ecmascript"] = "javascript",
        ["ts"] = "typescript",

        ["react.js"] = "react",
        ["reactjs"] = "react",
        ["react js"] = "react",
        ["vue.js"] = "vue",
        ["vuejs"] = "vue",
        ["vue js"] = "vue",
        ["node"] = "node.js",
        ["nodejs"] = "node.js",
        ["node js"] = "node.js",

        ["postgres"] = "postgresql",
        ["mongo"] = "mongodb",
        ["k8s"] = "kubernetes",

        ["rest"] = "rest api",
        ["restful"] = "rest api",
        ["rest apis"] = "rest api",
        ["restful api"] = "rest api",
        ["restful apis"] = "rest api",
        ["restful service"] = "rest api",
        ["restful services"] = "rest api",
        ["restful servis"] = "rest api",
        ["rest services"] = "rest api",
        ["restful web services"] = "rest api",

        ["unit test"] = "unit testing",
        ["unit tests"] = "unit testing",
        ["birim testi"] = "unit testing",
        ["birim testleri"] = "unit testing",

        ["ci cd"] = "ci/cd",
        ["cicd"] = "ci/cd",

        ["object oriented programming"] = "oop",
        ["nesne tabanlı programlama"] = "oop",
        ["nesne yönelimli programlama"] = "oop",

        ["large language models"] = "llm",
        ["large language model"] = "llm",
        ["llms"] = "llm",
        ["büyük dil modelleri"] = "llm",

        ["ml"] = "machine learning",
        ["microsoft azure"] = "azure",
        ["amazon web services"] = "aws",
        ["html5"] = "html",
        ["css3"] = "css",
        ["tailwind"] = "tailwind css",
        ["tailwindcss"] = "tailwind css",
        ["vs code"] = "visual studio code",
        ["vscode"] = "visual studio code",
    };

    // TEK YÖNLÜ çıkarım: adayda soldaki beceri varsa, sağdakini de karşılar.
    // (SQL Server bilen biri "SQL" isteyen ilana uyar; tersi geçerli değil.)
    private static readonly Dictionary<string, string[]> Implications = new()
    {
        ["sql server"] = ["sql"],
        ["postgresql"] = ["sql"],
        ["mysql"] = ["sql"],
        ["sqlite"] = ["sql"],
        ["asp.net core"] = [".net"],
        ["entity framework"] = [".net"],
        ["typescript"] = ["javascript"],
        ["react"] = ["javascript"],
        ["vue"] = ["javascript"],
        ["node.js"] = ["javascript"],
    };

    public static string Normalize(string? skill)
    {
        if (string.IsNullOrWhiteSpace(skill)) return string.Empty;

        var value = skill.ToLowerInvariant();
        value = Parenthetical.Replace(value, " ");
        value = value.Replace('-', ' ').Replace('_', ' ').Replace('/', ' ');
        value = Whitespace.Replace(value, " ").Trim();
        value = TrailingVersion.Replace(value, string.Empty).Trim();

        return Aliases.GetValueOrDefault(value, value);
    }

    // Adayın becerilerinin anahtarları + çıkarım yoluyla karşıladıkları
    public static HashSet<string> BuildCandidateKeys(IEnumerable<string> candidateSkills)
    {
        var keys = new HashSet<string>();

        foreach (var skill in candidateSkills)
        {
            var key = Normalize(skill);
            if (key.Length == 0) continue;

            keys.Add(key);

            if (Implications.TryGetValue(key, out var implied))
                keys.UnionWith(implied);
        }

        return keys;
    }
}