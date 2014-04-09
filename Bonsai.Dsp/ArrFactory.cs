using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    static class ArrFactory<TArray> where TArray : Arr
    {
        static readonly Lazy<Func<TArray, TArray>> templateFactory = new Lazy<Func<TArray, TArray>>(InitializeTemplateFactory);
        static readonly Lazy<Func<TArray, Depth, int, TArray>> templateSizeFactory = new Lazy<Func<TArray, Depth, int, TArray>>(InitializeTemplateSizeFactory);
        static readonly Lazy<Func<TArray, Depth, TArray>> templateSizeChannelFactory = new Lazy<Func<TArray, Depth, TArray>>(InitializeTemplateSizeChannelFactory);
        static readonly Lazy<Func<TArray, int, TArray>> templateSizeDepthFactory = new Lazy<Func<TArray, int, TArray>>(InitializeTemplateSizeDepthFactory);
        static readonly Lazy<Func<TArray, Size, TArray>> templateDepthChannelFactory = new Lazy<Func<TArray, Size, TArray>>(InitializeTemplateDepthChannelFactory);
        static readonly Lazy<Func<Size, Depth, int, TArray>> defaultFactory = new Lazy<Func<Size, Depth, int, TArray>>(InitializeDefaultFactory);

        public static Func<TArray, TArray> TemplateFactory
        {
            get { return templateFactory.Value; }
        }

        public static Func<TArray, Depth, int, TArray> TemplateSizeFactory
        {
            get { return templateSizeFactory.Value; }
        }

        public static Func<TArray, Depth, TArray> TemplateSizeChannelFactory
        {
            get { return templateSizeChannelFactory.Value; }
        }

        public static Func<TArray, int, TArray> TemplateSizeDepthFactory
        {
            get { return templateSizeDepthFactory.Value; }
        }

        public static Func<TArray, Size, TArray> TemplateDepthChannelFactory
        {
            get { return templateDepthChannelFactory.Value; }
        }

        public static Func<Size, Depth, int, TArray> DefaultFactory
        {
            get { return defaultFactory.Value; }
        }

        static Func<TArray, TArray> InitializeTemplateFactory()
        {
            LambdaExpression allocator;
            if (typeof(TArray) == typeof(Mat))
            {
                Expression<Func<Mat, Mat>> matAllocator = mat => new Mat(mat.Size, mat.Depth, mat.Channels);
                allocator = matAllocator;                
            }
            else if (typeof(TArray) == typeof(IplImage))
            {
                Expression<Func<IplImage, IplImage>> imageAllocator = image => new IplImage(image.Size, image.Depth, image.Channels);
                allocator = imageAllocator;
            }
            else throw new InvalidOperationException("Invalid array type.");
            return Expression.Lambda<Func<TArray, TArray>>(allocator.Body, allocator.Parameters).Compile();
        }

        static Func<TArray, Depth, int, TArray> InitializeTemplateSizeFactory()
        {
            LambdaExpression allocator;
            if (typeof(TArray) == typeof(Mat))
            {
                Expression<Func<Mat, Depth, int, Mat>> matAllocator =
                    (mat, depth, channels) => new Mat(mat.Size, depth, channels);
                allocator = matAllocator;
            }
            else if (typeof(TArray) == typeof(IplImage))
            {
                Expression<Func<IplImage, Depth, int, IplImage>> imageAllocator =
                    (image, depth, channels) => new IplImage(image.Size, ArrHelper.FromDepth(depth), channels);
                allocator = imageAllocator;
            }
            else throw new InvalidOperationException("Invalid array type.");
            return Expression.Lambda<Func<TArray, Depth, int, TArray>>(allocator.Body, allocator.Parameters).Compile();
        }

        static Func<TArray, Size, TArray> InitializeTemplateDepthChannelFactory()
        {
            LambdaExpression allocator;
            if (typeof(TArray) == typeof(Mat))
            {
                Expression<Func<Mat, Size, Mat>> matAllocator =
                    (mat, size) => new Mat(size, mat.Depth, mat.Channels);
                allocator = matAllocator;
            }
            else if (typeof(TArray) == typeof(IplImage))
            {
                Expression<Func<IplImage, Size, IplImage>> imageAllocator =
                    (image, size) => new IplImage(size, image.Depth, image.Channels);
                allocator = imageAllocator;
            }
            else throw new InvalidOperationException("Invalid array type.");
            return Expression.Lambda<Func<TArray, Size, TArray>>(allocator.Body, allocator.Parameters).Compile();
        }

        static Func<TArray, Depth, TArray> InitializeTemplateSizeChannelFactory()
        {
            LambdaExpression allocator;
            if (typeof(TArray) == typeof(Mat))
            {
                Expression<Func<Mat, Depth, Mat>> matAllocator =
                    (mat, depth) => new Mat(mat.Size, depth, mat.Channels);
                allocator = matAllocator;
            }
            else if (typeof(TArray) == typeof(IplImage))
            {
                Expression<Func<IplImage, Depth, IplImage>> imageAllocator =
                    (image, depth) => new IplImage(image.Size, ArrHelper.FromDepth(depth), image.Channels);
                allocator = imageAllocator;
            }
            else throw new InvalidOperationException("Invalid array type.");
            return Expression.Lambda<Func<TArray, Depth, TArray>>(allocator.Body, allocator.Parameters).Compile();
        }

        static Func<TArray, int, TArray> InitializeTemplateSizeDepthFactory()
        {
            LambdaExpression allocator;
            if (typeof(TArray) == typeof(Mat))
            {
                Expression<Func<Mat, int, Mat>> matAllocator =
                    (mat, channels) => new Mat(mat.Size, mat.Depth, channels);
                allocator = matAllocator;
            }
            else if (typeof(TArray) == typeof(IplImage))
            {
                Expression<Func<IplImage, int, IplImage>> imageAllocator =
                    (image, channels) => new IplImage(image.Size, image.Depth, channels);
                allocator = imageAllocator;
            }
            else throw new InvalidOperationException("Invalid array type.");
            return Expression.Lambda<Func<TArray, int, TArray>>(allocator.Body, allocator.Parameters).Compile();
        }

        static Func<Size, Depth, int, TArray> InitializeDefaultFactory()
        {
            LambdaExpression allocator;
            if (typeof(TArray) == typeof(Mat))
            {
                Expression<Func<Size, Depth, int, Mat>> matAllocator =
                    (size, depth, channels) => new Mat(size, depth, channels);
                allocator = matAllocator;
            }
            else if (typeof(TArray) == typeof(IplImage))
            {
                Expression<Func<Size, Depth, int, IplImage>> imageAllocator =
                    (size, depth, channels) => new IplImage(size, ArrHelper.FromDepth(depth), channels);
                allocator = imageAllocator;
            }
            else throw new InvalidOperationException("Invalid array type.");
            return Expression.Lambda<Func<Size, Depth, int, TArray>>(allocator.Body, allocator.Parameters).Compile();
        }
    }
}
