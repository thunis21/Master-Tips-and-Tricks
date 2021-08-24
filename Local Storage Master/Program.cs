using System;
using System.Diagnostics;

namespace Local_Storage_Master
{
    class Program
    {
        static void Main(string[] args)
        {

           var documentname= LocalStorage.GetSetting("MyApp", "Document", "LastOpened", "Doc-01");

            documentname = "test-doc";

            LocalStorage.SaveSetting("MyApp", "Document", "LastOpened", documentname);


            Debug.WriteLine(LocalStorage.GetSetting("MyApp", "Document", "LastOpened", "Doc-01"));

            var email = LocalStorage.Email;

            if(email == "?")
            {
                LocalStorage.Email = "joe@doe.com";
            }

            email = LocalStorage.Email;

            Debug.WriteLine(email);

        }
    }
}
