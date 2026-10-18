using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

namespace CareerAI.Tests.Integration;

// Tüm entegrasyon testleri TEK koleksiyonda: aynı sunucu/veritabanı paylaşılır ve testler sırayla çalışır
// (sahte AI sayaçları ve bayrakları paylaşılan durum olduğu için paralel çalışmamalı).
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<CareerApiFactory>
{
    public const string Name = "api";
}