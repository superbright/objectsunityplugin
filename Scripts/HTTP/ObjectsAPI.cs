using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.Objects.API
{
    public static class AWS
    {
        public const string AWS_ADDRESS = @"https://s3.amazonaws.com/";

        public const string S3Bucket = "s3deploy.test";

        public const string ThumbnailDir = "img";

        public static string ProjectPath
        {
            get { return Path.Combine(AWS.AWS_ADDRESS, AWS.S3Bucket); }
        }

        public static string ThumbnailRoot
        {
            get { return Path.Combine(ProjectPath, AWS.ThumbnailDir); }
        } 
    }
}
 