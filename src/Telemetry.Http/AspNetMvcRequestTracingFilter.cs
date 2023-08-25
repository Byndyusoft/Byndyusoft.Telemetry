using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Byndyusoft.MaskedSerialization.Newtonsoft.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// TODO Remove Tracing
namespace Byndyusoft.AspNetCore.Mvc.Telemetry.Http
{
    public class AspNetMvcRequestTracingFilter : IAsyncActionFilter
    {
        private readonly AspNetMvcTracingOptions _options;

        public AspNetMvcRequestTracingFilter(
            IOptions<AspNetMvcTracingOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _options = options.Value;
        }

        public Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            return OnActionExecutionAsync(context, next, context.HttpContext.RequestAborted);
        }

        private async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next,
            CancellationToken cancellationToken)
        {
            var telemetryInfoItemCollection = new TelemetryInfoItemCollection(
                TelemetryHttpProviderUniqueNames.Request,
                "Action Executing")
            {
                { "http.request.header.accept", context.HttpContext.Request.Headers["accept"].ToArray() },
                { "http.request.header.content_type", context.HttpContext.Request.ContentType },
                { "http.request.header.content_length", context.HttpContext.Request.ContentLength }
            };

            foreach (var (name, value) in GetParameters(context))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var json = await _options.FormatAsync(value, cancellationToken)
                    .ConfigureAwait(false);
                telemetryInfoItemCollection.Add($"http.request.params.{name}", json);
            }

            TelemetryRouter.WriteTelemetryInfo(telemetryInfoItemCollection);

            await next();
        }

        private static IEnumerable<(string name, object? value)> GetParameters(ActionExecutingContext context)
        {
            foreach (var actionParameter in context.ActionDescriptor.Parameters)
            {
                if (actionParameter.BindingInfo?.BindingSource == BindingSource.Services ||
                    actionParameter.BindingInfo?.BindingSource == BindingSource.Special)
                    continue;

                var name = actionParameter.Name;
                if (context.ActionArguments.TryGetValue(actionParameter.Name, out var value))
                {
                    yield return (name, value);
                }
            }
        }
    }

    public class AspNetMvcTracingOptions
    {
        public const int DefaultValueMaxStringLength = 2000;

        private IFormatter _formatter;
        private int? _valueMaxStringLength;

        public AspNetMvcTracingOptions()
        {
            _formatter = new NewtonsoftJsonFormatter();
            _valueMaxStringLength = DefaultValueMaxStringLength;
        }

        public IFormatter Formatter
        {
            get => _formatter;
            set => _formatter = value ?? throw new ArgumentNullException(nameof(Formatter));
        }

        public int? ValueMaxStringLength
        {
            get => _valueMaxStringLength;
            set => _valueMaxStringLength = value ?? throw new ArgumentNullException(nameof(ValueMaxStringLength));
        }

        internal ValueTask<string?> FormatAsync(object? value, CancellationToken cancellationToken = default)
        {
            return Formatter.FormatAsync(value, this, cancellationToken);
        }

        internal void Configure(AspNetMvcTracingOptions options)
        {
            Formatter = options.Formatter;
            ValueMaxStringLength = options.ValueMaxStringLength;
        }
    }

    public interface IFormatter
    {
        ValueTask<string?> FormatAsync(object? value, AspNetMvcTracingOptions options,
            CancellationToken cancellationToken);
    }

    public abstract class FormatterBase : IFormatter
    {
        public async ValueTask<string?> FormatAsync(object? value, AspNetMvcTracingOptions options,
            CancellationToken cancellationToken = default)
        {
            if (value is null)
                return "null";

            using var stream = new StringLimitStream(options.ValueMaxStringLength);

            await FormatValueAsync(value, stream, options, cancellationToken)
                .ConfigureAwait(false);

            return stream.GetString();
        }

        protected abstract ValueTask FormatValueAsync(object value, Stream stream, AspNetMvcTracingOptions options,
            CancellationToken cancellationToken);
    }

    public class NewtonsoftJsonFormatter : FormatterBase
    {
        private JsonSerializerSettings _settings;

        public NewtonsoftJsonFormatter()
        {
            _settings = new JsonSerializerSettings();
            _settings.SetupSettingsForMaskedSerialization();
        }

        public JsonSerializerSettings Settings
        {
            get => _settings;
            set => _settings = value ?? throw new ArgumentNullException(nameof(Settings));
        }

        protected override async ValueTask FormatValueAsync(
            object value,
            Stream stream,
            AspNetMvcTracingOptions options,
            CancellationToken cancellationToken)
        {
            var writer = new StreamWriter(stream, null!, -1, true);
            using var jsonWriter = new JsonTextWriter(writer);

            var serializer = JsonSerializer.Create(Settings);
            serializer.Serialize(jsonWriter, value);

            await jsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    internal class StringLimitStream : Stream
    {
        private readonly int? _lengthLimit;
        private MemoryStream? _memory;
        private bool _oversized;

        public StringLimitStream(int? lengthLimit)
        {
            _lengthLimit = lengthLimit;
            _memory = new MemoryStream();
        }

        [ExcludeFromCodeCoverage] public override bool CanRead => Inner.CanRead;

        [ExcludeFromCodeCoverage] public override bool CanSeek => Inner.CanSeek;

        [ExcludeFromCodeCoverage] public override bool CanWrite => Inner.CanWrite;

        [ExcludeFromCodeCoverage] public override long Length => Inner.Length;

        [ExcludeFromCodeCoverage]
        public override long Position
        {
            get => Inner.Position;
            set => Inner.Position = value;
        }

        private MemoryStream Inner => _memory ?? throw new ObjectDisposedException(nameof(StringLimitStream));

        [ExcludeFromCodeCoverage]
        public override void Flush()
        {
            Inner.Flush();
        }

        [ExcludeFromCodeCoverage]
        public override int Read(byte[] buffer, int offset, int count)
        {
            return Inner.Read(buffer, offset, count);
        }

        [ExcludeFromCodeCoverage]
        public override long Seek(long offset, SeekOrigin origin)
        {
            return Inner.Seek(offset, origin);
        }

        [ExcludeFromCodeCoverage]
        public override void SetLength(long value)
        {
            Inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var inner = Inner;

            if (_lengthLimit != null && inner.Length + count > _lengthLimit)
            {
                _oversized = true;
                count = _lengthLimit.Value - (int)inner.Length;
            }

            Inner.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _memory?.Dispose();
                _memory = null;
            }

            base.Dispose(disposing);
        }

        public string GetString()
        {
            var str = Encoding.UTF8.GetString(Inner.ToArray());
            return _oversized ? $"{str}..." : str;
        }
    }

    public static class TelemetryHttpProviderUniqueNames
    {
        public static string Request => "HttpRequest";
    }

    /// <summary>
    ///     Extension methods for adding Tracing to MVC.
    /// </summary>
    public static class TracingMvcBuilderExtensions
    {
        /// <returns>The <see cref="IMvcBuilder" />.</returns>
        public static IMvcBuilder AddTracing(this IMvcBuilder builder,
            Action<AspNetMvcTracingOptions>? configure = null)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return builder
                .AddRequestTracing(configure);
                //.AddResponseTracing(configure);
        }

        /// <returns>The <see cref="IMvcCoreBuilder" />.</returns>
        public static IMvcCoreBuilder AddTracing(this IMvcCoreBuilder builder,
            Action<AspNetMvcTracingOptions>? configure = null)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return builder
                .AddRequestTracing(configure);
                //.AddResponseTracing(configure);
        }

        /// <returns>The <see cref="IMvcBuilder" />.</returns>
        public static IMvcBuilder AddRequestTracing(this IMvcBuilder builder,
            Action<AspNetMvcTracingOptions>? configure = null)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddRequestTracingCore(configure);

            return builder;
        }

        /// <returns>The <see cref="IMvcCoreBuilder" />.</returns>
        public static IMvcCoreBuilder AddRequestTracing(this IMvcCoreBuilder builder,
            Action<AspNetMvcTracingOptions>? configure = null)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddRequestTracingCore(configure);

            return builder;
        }

        ///// <returns>The <see cref="IMvcBuilder" />.</returns>
        //public static IMvcBuilder AddResponseTracing(this IMvcBuilder builder,
        //    Action<AspNetMvcTracingOptions>? configure = null)
        //{
        //    if (builder is null)
        //        throw new ArgumentNullException(nameof(builder));

        //    builder.Services.AddResponseTracingCore(configure);

        //    return builder;
        //}

        ///// <returns>The <see cref="IMvcCoreBuilder" />.</returns>
        //public static IMvcCoreBuilder AddResponseTracing(this IMvcCoreBuilder builder,
        //    Action<AspNetMvcTracingOptions>? configure = null)
        //{
        //    if (builder is null)
        //        throw new ArgumentNullException(nameof(builder));

        //    builder.Services.AddResponseTracingCore(configure);

        //    return builder;
        //}

        //private static void AddResponseTracingCore(this IServiceCollection services,
        //    Action<AspNetMvcTracingOptions>? configure)
        //{
        //    if (configure != null)
        //    {
        //        services.Configure(configure);
        //    }

        //    services.PostConfigure<MvcOptions>(options =>
        //    {
        //        options.Filters.Add<AspNetMvcResponseTracingFilter>();
        //    });
        //}

        private static void AddRequestTracingCore(this IServiceCollection services,
            Action<AspNetMvcTracingOptions>? configure)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }

            services.PostConfigure<MvcOptions>(options =>
            {
                options.Filters.Add<AspNetMvcRequestTracingFilter>();
            });
        }
    }
}