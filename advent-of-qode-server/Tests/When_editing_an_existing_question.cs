using System;
using System.Collections.Generic;
using advent_of_qode_server;
using advent_of_qode_server.Controllers;
using advent_of_qode_server.Domain;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            _queryController = new QueryController(adventContext);
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

            var response = await _queryController.EditExistingQuestion(queryInput);
            response.Should().BeOfType(typeof(OkObjectResult));
        }

        [Fact]
        public async void Should_return_not_found_if_entered_wrong_day()
        {
            var queryInput = new QuestionInputModel
            {
                Day = 2,
                Question = "Is earth round?",
                Options = new List<OptionInputModel>
                {
                    new() { Text = "Yes", IsCorrectAnswer = true },
                }
            };

            var response = await _queryController.EditExistingQuestion(queryInput);
            response.Should().BeOfType(typeof(NotFoundObjectResult));
        }
    }
}
