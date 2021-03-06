﻿using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Models;
using Newtonsoft.Json;
using UnityEngine;

public static class UserService
{
    #region Fields and properties

    #endregion

    #region Public methods
    public static void CreateUser(string userName, HttpClient client)
    {
        try
        {
            var user = new User("tempId", userName, Constants.UnityCustomTokenAPI);
            string userJson = JsonConvert.SerializeObject(user);
            var content = new StringContent(userJson.ToString(), Encoding.UTF8, "application/json");

            var response = client.PostAsync(
                $"{Constants.ApiUrl + Constants.ApiServiceUserCreate}",
                content)
                .Result;

            response.EnsureSuccessStatusCode();

            PlayerPrefs.SetString(Constants.PlayerPrefKeyUser, userName);
        }
        catch (System.Exception ex)
        {
            throw ex;
        }
    }

    public static bool DoesUserExist(string userName, HttpClient client)
    {
        var methodString = string.Format(Constants.ApiServiceUserDoesUserExist, userName);

        var response = client.GetStringAsync(
               $"{Constants.ApiUrl + methodString}")
               .Result;

        return bool.Parse(response.ToString());
    }

    #endregion


    #region Private methods	


    #endregion
}
