using System;
using System.Collections.Generic;
using System.Net;
using advent_of_qode_server;
using advent_of_qode_server.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Tests
{
    public class When_creating_a_new_question_with_incorrect_input_2
    {
        private QueryController _queryController;

        public When_creating_a_new_question_with_incorrect_input_2()
        {
            var adventOptions = new DbContextOptionsBuilder<AdventContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var adventContext = new AdventContext(adventOptions);

            var fakeConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>())
                .Build();

            _queryController = new QueryController(adventContext, fakeConfig);
        }

        [Fact]
        public async void Should_return_bad_request_for_not_having_a_correct_answer_supplied()
        {
            var queryInput = new QuestionInputModel
            {
                Day = 1,
                Options = new List<OptionInputModel>
                {
                    new() { Text = "First Wrong Answer", IsCorrectAnswer = false },
                    new() { Text = "Also Wrong Answer", IsCorrectAnswer = false }
                }
            };

            var response = await _queryController.AddOrUpdateQuestion(queryInput) as BadRequestObjectResult;
            response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            response.Value.Should().Be("Must have at least one correct answer");
        }
    }
}