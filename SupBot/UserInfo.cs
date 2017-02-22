using System;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;

namespace SupportBot
{
    public enum DepartmentOptions
    {
        Accounting,
        AdministrativeSupport,
        IT
    }

    [Serializable]
    public class UserInfo
    {
        [Prompt("Please enter your {&}.")]
        public string Name;

        [Prompt("Please enter your {&}.")]
        public string PhoneNumber;

        [Prompt("Please enter your {&}.")]
        [Pattern(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")]
        public string EmailAddress;

        [Prompt("What {&} is this request? {||}")]
        public DepartmentOptions? Department;

        public static IForm<UserInfo> BuildForm()
        {
            return new FormBuilder<UserInfo>().Build();
        }
    }
}
