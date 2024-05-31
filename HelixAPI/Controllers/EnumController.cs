using Microsoft.AspNetCore.Mvc;
using HelixAPI.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HelixAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnumsController : ControllerBase
    {
        [HttpGet("api/v1/enum/catagory")]
        public IActionResult GetCatagoryValues()
        {
            var enumValues = Enum.GetValues(typeof(Catagory))
                                 .Cast<Catagory>()
                                 .Select(e => new { Id = (int)e, Name = e.ToString() })
                                 .ToList();

            return Ok(enumValues);
        }

        [HttpGet("api/v1/enum/branch")]
        public IActionResult GetBranchValues()
        {
            var enumValues = Enum.GetValues(typeof(Branch))
                                 .Cast<Branch>()
                                 .Select(e => new { Id = (int)e, Name = e.ToString() })
                                 .ToList();

            return Ok(enumValues);
        }

        [HttpGet("api/v1/enum/content_type")]
        public IActionResult GetContentTypeValues()
        {
            var enumValues = Enum.GetValues(typeof(ContentType))
                                 .Cast<ContentType>()
                                 .Select(e => new { Id = (int)e, Name = e.ToString() })
                                 .ToList();

            return Ok(enumValues);
        }

        [HttpGet("api/v1/enum/flag")]
        public IActionResult GetFlagValues()
        {
            var enumValues = Enum.GetValues(typeof(Flag))
                                 .Cast<Flag>()
                                 .Select(e => new { Id = (int)e, Name = e.ToString() })
                                 .ToList();

            return Ok(enumValues);
        }

        [HttpGet("api/v1/enum/format")]
        public IActionResult GetMyEnumValues()
        {
            var enumValues = Enum.GetValues(typeof(Format))
                                 .Cast<Format>()
                                 .Select(e => new { Id = (int)e, Name = e.ToString() })
                                 .ToList();

            return Ok(enumValues);
        }

        [HttpGet("api/v1/enum/subject")]
        public IActionResult GetSubjectValues()
        {
            var enumValues = Enum.GetValues(typeof(Subject))
                                 .Cast<Subject>()
                                 .Select(e => new { Id = (int)e, Name = e.ToString() })
                                 .ToList();

            return Ok(enumValues);
        }

        [HttpGet("api/v1/enum/relationship_type")]
        public IActionResult GetRelationshipTypeValues()
        {
            var enumValues = Enum.GetValues(typeof(RelationshipType))
                                 .Cast<RelationshipType>()
                                 .Select(e => new { Id = (int)e, Name = e.ToString() })
                                 .ToList();

            return Ok(enumValues);
        }
    }
}