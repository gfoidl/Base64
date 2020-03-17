using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("gfoidl.Base64.Benchmarks, Publickey=" + Keys.SignKey)]
[assembly: InternalsVisibleTo("gfoidl.Base64.FuzzTests, Publickey="  + Keys.SignKey)]
[assembly: InternalsVisibleTo("gfoidl.Base64.Tests, Publickey="      + Keys.SignKey)]

internal static class Keys
{
    public const string SignKey = "00240000048000009400000006020000002400005253413100040000010001009102c074550e276c36b61e473783fb81bece760951fe0d55c21fe7d8296e174cb41fb4f57a91544f3d597ba044e0278fb2fbb5af1b6fa697e20ca1707f907bb687b73b6ef8ec578404ed49d2206d0f485d4073b88722d92e1f1018e2467800e760b1a8ee478aec551b2975e01ae25140debfae1680042cdd4c80950909a8e8fc";
}
