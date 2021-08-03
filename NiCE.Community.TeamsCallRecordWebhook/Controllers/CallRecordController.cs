// <copyright file="CallRecordController.cs" company="NiCE IT Management Solutions GmbH">
// Copyright (c) NiCE IT Management Solutions GmbH. All rights reserved.
// </copyright>

namespace NiCE.Community.TeamsCallRecordWebhook.Controllers
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Graph;

    /// <summary>
    /// Api Controller to manage the subscriptions.
    /// </summary>
    [ApiController]

    // [Route("api/[controller]")]
    [Route(HttpRouteConstants.CallRecordController)]
    public class CallRecordController : ControllerBase
    {
        private readonly CallRecordCache cache;
        private readonly Serializer serializer = new ();

        /// <summary>
        /// Initializes a new instance of the <see cref="CallRecordController"/> class.
        /// </summary>
        /// <param name="cache">The Cache of Call Records.</param>
        public CallRecordController(CallRecordCache cache)
        {
            this.cache = cache;
        }

        /// <summary>
        /// Endpoint to get all CallRecord contents ordered by ID.
        /// </summary>
        /// <returns>returns all CallRecord contents ordered by ID.</returns>
        [HttpGet]
        [Route("")]
        public ContentResult Get()
        {
            return new ContentResult()
            {
                Content = this.serializer.SerializeObject(this.cache.CallRecords.Select(r => r.Id).ToArray()),
                ContentType = "application/json",
                StatusCode = 200,
            };
        }

        /// <summary>
        /// Endpoint to get all CallRecord contents.
        /// </summary>
        /// <returns>returns all call record contents.</returns>
        [HttpGet]
        [Route("all")]
        public ContentResult AllGet()
        {
            return new ContentResult()
            {
                Content = this.serializer.SerializeObject(this.cache.CallRecords.ToArray()),
                ContentType = "application/json",
                StatusCode = 200,
            };
        }

        /// <summary>
        /// Endpoint to get one CallRecord content by ID.
        /// </summary>
        /// <param name="id">The id of the Call record.</param>
        /// <returns>returns one CallRecord content by ID.</returns>
        [HttpGet]
        [Route("{id:guid}")]
        public ContentResult Get(Guid id)
        {
            var ids = id.ToString();

            var found = this.cache.CallRecords.FindAll(r => r.Id.Equals(ids));

            if (found.Count > 0)
            {
                return new ContentResult()
                {
                    Content = this.serializer.SerializeObject(found.First()),
                    ContentType = "application/json",
                    StatusCode = 200,
                };
            }
            else
            {
                return new ContentResult()
                {
                    Content = "No Record Found",
                    ContentType = "text/plain",
                    StatusCode = 404,
                };
            }
        }
    }
}
