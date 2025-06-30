using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Web;

namespace SPACEGO_E_COMMERCE_WEBSITE.Libraries
{
    public class VnPayLibrary
    {
        public const string VERSION = "2.1.0";
        private SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }

        public string GetResponseData(string key)
        {
            _responseData.TryGetValue(key, out var retValue);
            return retValue;
        }

        public string GetIpAddress(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString();
        }

        public string CreateRequestUrl(string baseUrl, string vnp_HashSecret)
        {
            var data = new StringBuilder();
            foreach (var kv in _requestData)
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }

            var queryString = data.ToString().TrimEnd('&');
            var signData = CreateHash(vnp_HashSecret, queryString);

            return $"{baseUrl}?{queryString}&vnp_SecureHash={signData}";
        }

        public string CreateHash(string key, string inputData)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(inputData));
            var hex = BitConverter.ToString(hashValue).Replace("-", "");
            return hex;
        }

        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string vnp_HashSecret)
        {
            var vnpayData = new SortedList<string, string>(new VnPayCompare());

            foreach (var key in collection.Keys)
            {
                if (key.StartsWith("vnp_"))
                {
                    vnpayData.Add(key, collection[key]);
                }
            }

            if (vnpayData.TryGetValue("vnp_SecureHash", out var vnp_SecureHash))
            {
                vnpayData.Remove("vnp_SecureHash");
            }

            var signData = new StringBuilder();
            foreach (var kv in vnpayData)
            {
                signData.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }

            string checkSum = CreateHash(vnp_HashSecret, signData.ToString().TrimEnd('&'));

            var response = new PaymentResponseModel
            {
                Success = checkSum.Equals(vnp_SecureHash, StringComparison.InvariantCultureIgnoreCase),
                VnPayData = vnpayData
            };

            return response;
        }

        private class VnPayCompare : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return string.CompareOrdinal(x, y);
            }
        }
    }

    public class PaymentResponseModel
    {
        public bool Success { get; set; }
        public SortedList<string, string> VnPayData { get; set; }
    }
}
