using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using advent_of_qode_server;
using advent_of_qode_server.Controllers;
using advent_of_qode_server.Domain;
using advent_of_qode_server.Logic;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Tests
{
    public class When_editing_an_existing_question
    {
        private readonly QueryController _queryController;
        string _admin1 = "admin@admin.se";

        public When_editing_an_existing_question()
        {
            var options = new DbContextOptionsBuilder<AdventContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var adventContext = new AdventContext(options);
            adventContext.Questions.Add(new Question
            {
                Day = 1,
                Year = DateTime.UtcNow.Year,
                Query = "Is the sky orange?",
                Options = new List<Option>
                    {
                        new()
                        {
                            IsCorrectAnswer = true,
                            Text = "Sometimes",
                        },
                        new()
                        {
                            IsCorrectAnswer = false,
                            Text = "No",
                        }
                    }
            });
            adventContext.SaveChanges();
            var _admin1 = "admin@admin.se";
            var fakeConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    {"Uniqode:Admins",_admin1}
                })
                .Build();

            var fakeGoogleService = A.Fake<IGoogleService>();
            var fakeScoreService = A.Fake<IScoreService>();

            A.CallTo(() => fakeGoogleService.GetEmailByGmailToken(A<StringValues>.Ignored, A<string>.Ignored))
                .Returns(Task.FromResult(_admin1));

            _queryController = new QueryController(adventContext, fakeConfig, fakeGoogleService, fakeScoreService);
        }

        [Fact]
        public async void Should_return_successful()
        {
            var queryInput = new QuestionInputModel
            {
                Day = 1,
                Question = "Is earth round?",
                Options = new List<OptionInputModel>
                {
                    new() { Text = "Yes", IsCorrectAnswer = true },
                }
            };

            var response = await _queryController.AddOrUpdateQuestion(queryInput);
            response.Should().BeOfType(typeof(OkObjectResult));

            var getResponse = await _queryController.GetQuestion(1, _admin1) as OkObjectResult;
            var question = getResponse.Value as QuestionViewModel;
            question.Question.Should().Be("Is earth round?");
            question.Options.Length.Should().Be(1);
            question.Options.Single().Should().Be("Yes");
        }
    }
}
