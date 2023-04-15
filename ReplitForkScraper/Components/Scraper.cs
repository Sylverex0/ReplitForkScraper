using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReplitForkScraper.Components
{
    public class Scraper
    {
        public static async Task<List<string>> GrabForks(string repl_slug)
        {
            List<string> forks = new List<string>();

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync($"https://replit.com/data/repls/{repl_slug}");

                var content = await response.Content.ReadAsStringAsync();
                var replData = JsonConvert.DeserializeObject<dynamic>(content);

                const string search_query = @"
                    query PublicForks($replId: String!, $count: Int!, $after: String) {
                        repl(id: $replId) {
                            ... on Repl {
                                id
                                publicForkCount
                                releasesForkCount
                                publicForks(count: $count, after: $after) {
                                    items {
                                        id
                                        ...PublicForkReplItem
                                        __typename
                                    }
                                    pageInfo {
                                        nextCursor
                                        __typename
                                    }
                                    __typename
                                }
                                __typename
                            }
                            __typename
                        }
                    }
                    fragment PublicForkReplItem on Repl {
                        id
                        ...ReplPostReplCardRepl
                        __typename
                    }
                    fragment ReplPostReplCardRepl on Repl {
                        id
                        iconUrl
                        description(plainText: true)
                        ...ReplPostReplInfoRepl
                        ...ReplStatsRepl
                        ...ReplLinkRepl
                        tags {
                            id
                            ...PostsFeedNavTag
                            __typename
                        }
                        owner {
                            ... on Team {
                                id
                                username
                                url
                                image
                                __typename
                            }
                            ... on User {
                                id
                                username
                                url
                                image
                                __typename
                            }
                            __typename
                        }
                        __typename
                    }
                    fragment ReplPostReplInfoRepl on Repl {
                        id
                        title
                        description(plainText: true)
                        imageUrl
                        iconUrl
                        templateInfo {
                            label
                            iconUrl
                            __typename
                        }
                        __typename
                    }
                    fragment ReplStatsRepl on Repl {
                        id
                        likeCount
                        runCount
                        commentCount
                        __typename
                    }
                    fragment ReplLinkRepl on Repl {
                        id
                        url
                        nextPagePathname
                        __typename
                    }
                    fragment PostsFeedNavTag on Tag {
                        id
                        isOfficial
                        __typename
                    }
                ";

                string query = JsonConvert.SerializeObject(new
                {
                    query = search_query,
                    variables = new
                    {
                        replId = replData["id"],
                        count = 100
                    }
                });

                var requestContent = new StringContent(query, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Add("Origin", "https://replit.com");
                client.DefaultRequestHeaders.Add("Referer", $"https://replit.com{repl_slug}");
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

                HttpResponseMessage grabForks = await client.PostAsync("https://replit.com/graphql", requestContent);

                string other = await grabForks.Content.ReadAsStringAsync();

                string jsonContent = await grabForks.Content.ReadAsStringAsync();
                dynamic forkData = JsonConvert.DeserializeObject<dynamic>(jsonContent);

                for (int i = 0; i < forkData["data"]["repl"]["publicForks"]["items"].Count; i++)
                {
                    string url = forkData["data"]["repl"]["publicForks"]["items"][i]["url"];
                    forks.Add($"https://replit.com{url}");
                }
            }

            return forks;
        }
    }
}