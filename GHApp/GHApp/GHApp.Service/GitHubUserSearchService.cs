﻿using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using GHApp.Contracts;
using GHApp.Contracts.Dto;
using Newtonsoft.Json;

namespace GHApp.Service
{
    public class GitHubUserSearchService : IGitHubUserSearchService
    {
        private readonly IHttpClientFactory _factory;

        public GitHubUserSearchService(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public IObservable<IEnumerable<User>> FindUser(string userNamePart)
        {
            string queryString = "https://api.github.com/search/users?q={0}&page={1}&per_page=10";

            int readRecords = 0;
            int currentPage = 0;
            bool hasMoreData = true;

            return Observable.Using(_factory.CreateHttpClient, client =>
            {
                return Observable.Create<IEnumerable<User>>(async (o, token) =>
                {
                    while (!token.IsCancellationRequested && hasMoreData)
                    {
                        var request = string.Format(queryString, userNamePart, 0);
                        var resultString = await client.GetStringAsync(request);
                        var result = JsonConvert.DeserializeObject<SearchResult>(resultString);

                        readRecords += result.Users.Length;
                        ++currentPage;

                        hasMoreData = readRecords < result.TotalCount;
                        o.OnNext(result.Users);
                    }

                    Console.WriteLine("Exiting - cancellation: {0}", token);
                });
            });
        }
    }
}