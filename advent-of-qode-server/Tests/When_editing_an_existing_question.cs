using System;
using System.Collections.Generic;
using System.Linq;
using advent_of_qode_server;
using advent_of_qode_server.Controllers;
using advent_of_qode_server.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Tests
{
    public class When_editing_an_existing_question
    {
        private readonly QueryController _queryController;

        public When_editing_an_existing_question()
        {
            var options = new DbContextOptionsBuilder<AdventContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var adventContext = new AdventContext(options);
            adventContext.Questions.Add(new Question
            {
                Day = 1,
                Year = DateTime.Now.Year,
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
            var fakeConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            _queryController = new QueryController(adventContext, fakeConfig);
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

            var getResponse = await _queryController.GetQuestion(1) as OkObjectResult;
            var question = getResponse.Value as QuestionViewModel;
            question.Question.Should().Be("Is earth round?");
            question.Options.Length.Should().Be(1);
            question.Options.Single().Should().Be("Yes");
        }
    }
}
