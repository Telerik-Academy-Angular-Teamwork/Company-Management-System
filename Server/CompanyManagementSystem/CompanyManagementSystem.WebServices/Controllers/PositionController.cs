﻿namespace CompanyManagementSystem.WebServices.Controllers
{
    using Abstraction;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Services.Abstraction;
    using System.Linq;

    [EnableCors("MyPolicy")]
    [Route("api/[controller]")]
    public class PositionController : BaseController
    {
        private readonly IPositionService service;

        public PositionController(IPositionService service)
        {
            this.service = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var result = this.service
                .GetAll()
                .ToList();

            return this.Ok(result);
        }
    }
}