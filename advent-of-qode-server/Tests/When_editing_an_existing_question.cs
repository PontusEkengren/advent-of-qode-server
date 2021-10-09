using System;
using System.Collections.Generic;
using System.Net;
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
            //Arrange
            var options = new DbContextOptionsBuilder<AdventContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var adventContext = new AdventContext(options);
            adventContext.Questions.Add(new Question
            {
                Day = 1,
                Answer = "Is sky orange?",
                Options = new List<Option>
                    {
                        new Option
                        {
                            IsCorrectAnswer = true,
                            Text = "Sometimes",
                        },
                        new Option
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
        public async void Should_return_not_found()
        {
            //Act
            var queryInput = new QuestionInputModel
            {
                Day = 2,
                Question = "Is earth round?",
                Options = new List<OptionInputModel>
                {
                    new OptionInputModel { Text = "Yes", IsCorrectAnswer = true },
                }
            };

            var response = await _queryController.EditExistingQuestion(queryInput);
            response.Should().BeOfType(typeof(NotFoundObjectResult));
        }
    }
}
