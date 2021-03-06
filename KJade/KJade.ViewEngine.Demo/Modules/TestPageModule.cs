﻿using Nancy;

namespace KJade.ViewEngine.Demo.Modules
{
    public class TestPageModule : NancyModule
    {
        public TestPageModule()
        {
            Get("/test", args =>
            {
                var someList = new[] { "something1", "thing2", "thing3" };
                var model = new
                {
                    Name = "Bob",
                    SomeEnumerable = someList,
                    SomeFlag = true,
                    SomeOtherFlag = false,
                    PTestModel = new
                    {
                        SomeMsg = "you are awsum",
                    }
                };
                return View["Test", model];
            });
        }
    }
}