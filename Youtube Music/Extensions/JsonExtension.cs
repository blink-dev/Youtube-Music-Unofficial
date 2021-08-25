using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Youtube_Music.Extensions
{
    public static class JsonExtension
    {
        public static List<JToken> FindTokens(this JToken containerToken, string name, int findCount = 0)
        {
            List<JToken> matches = new();
            FindTokens(containerToken, name, matches, findCount);
            return matches;
        }

        private static void FindTokens(JToken containerToken, string name, List<JToken> matches, int findCount = 0)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    if (child.Name == name)
                    {
                        matches.Add(child.Value);
                        if (findCount > 0)
                        {
                            if (matches.Count == findCount) return;
                        }
                    }
                    FindTokens(child.Value, name, matches, findCount);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindTokens(child, name, matches, findCount);
                    if (matches.Count == findCount) return;
                }
            }
        }
    }
}
