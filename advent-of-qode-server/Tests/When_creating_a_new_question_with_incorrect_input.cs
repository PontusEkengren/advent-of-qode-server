using System;
using System.Collections.Generic;
using System.Net;
using advent_of_qode_server;
using advent_of_qode_server.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests
{
    public class When_creating_a_new_question_with_incorrect_input
    {
        private QueryController _queryController;

        public When_creating_a_new_question_with_incorrect_input()
        {
            var adventOptions = new DbContextOptionsBuilder<AdventContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var adventContext = new AdventContext(adventOptions);
            _queryController = new QueryController(adventContext);
        }

        [Fact]
        public async void Should_return_bad_request_for_missing_question()
        {
            var queryInput = new QuestionInputModel
            {
                Day = 1,
                Options = new List<OptionInputModel>
                {
                    new() { Text = "No, fabricated by CocaCola!", IsCorrectAnswer = false },
                    new() { Text = "Certainly", IsCorrectAnswer = true }
                }
            };

            var response = await _queryController.AddOrUpdateQuestion(queryInput) as BadRequestObjectResult;
            response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async void Should_return_bad_request_for_missing_answers()
        {
            var queryInput = new QuestionInputModel
            {
                Day = 1,
                Question = "Is Santa Real?",
            };

            var response = await _queryController.AddOrUpdateQuestion(queryInput) as BadRequestObjectResult;
            response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }
    }
}