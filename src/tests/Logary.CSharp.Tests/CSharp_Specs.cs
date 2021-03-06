// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedMember.Global

namespace Logary.Specs
{
  using System;
  using System.Text;
  using System.Text.RegularExpressions;

  using Configuration;
  using Targets;

  using Machine.Specifications;

  using NodaTime;

  public class When_configuring_with_CSharp_API
  {
    private Establish context = () =>
      {
        writer = new System.IO.StringWriter(new StringBuilder());
        timestamp = Instant.FromSecondsSinceUnixEpoch(987654);
        exception = new ApplicationException("Nice exception");

        manager = LogaryFactory.New(
          "Logary Specs",
          with =>
          with.Target<TextWriter.Builder>(
            "sample string writer",
            t =>
            t.Target.WriteTo(writer, writer)
              .MinLevel(LogLevel.Verbose)
              .SourceMatching(new Regex(".*")))).Result;
      };

    Cleanup cleanup = () =>
      {
        manager.Dispose();
        writer.Dispose();
      };

    Because reason = () =>
      {
        var logger = manager.GetLogger("Logary.Specs.When_configuring_with_CSharp_API");
        logger.LogEvent(LogLevel.Warn, "the situation is dire", new { foo = "oh-noes" }, timestamp, exception);
        
        manager.FlushPending(Duration.FromSeconds(8L)).Wait();
        subject = writer.ToString();
      };

    It output_should_contain_message = () => subject.ShouldContain("the situation is dire");
    It output_should_contain_the_field = () => subject.ShouldContain("oh-noes");
    It output_should_contain_timestamp = () => subject.ShouldContain(timestamp.Ticks.ToString());
    It output_should_contain_exception = () => subject.ShouldContain(exception.Message);

    static LogManager manager;
    static System.IO.StringWriter writer;
    static string subject;
    static Instant timestamp;
    static Exception exception;
  }

  public class When_configuring_with_CSharp_API_and_using_setter_transformer
  {
    private Establish context = () =>
    {
      writer = new System.IO.StringWriter(new StringBuilder());
      timestamp = Instant.FromSecondsSinceUnixEpoch(987654);
      exception = new ApplicationException("Nice exception");

      manager = LogaryFactory.New(
        "Logary Specs",
        with =>
        with.Target<TextWriter.Builder>(
          "sample string writer",
          t =>
          t.Target.WriteTo(writer, writer)
            .MinLevel(LogLevel.Verbose)
            .SourceMatching(new Regex(".*")))).Result;
    };

    Cleanup cleanup = () =>
    {
      manager.Dispose();
      writer.Dispose();
    };

    Because reason = () =>
    {
      var logger = manager.GetLogger("Logary.When_configuring_with_CSharp_API_and_using_setter_transformer");

      logger.LogEvent(
        LogLevel.Warn,
        "the situation is dire",
        msg => msg
                 .SetFieldsFromObject(new { foo = "oh-noes" })
                 .SetTimestamp(timestamp)
                 .AddException(exception)
                 .SetContextFromObject(new {contextdata = "the Contextdata"})
        );

      manager.FlushPending(Duration.FromSeconds(8L)).Wait();
      subject = writer.ToString();
    };

    It output_should_contain_message = () => subject.ShouldContain("the situation is dire");
    It output_should_contain_the_field = () => subject.ShouldContain("oh-noes");
    It output_should_contain_timestamp = () => subject.ShouldContain(timestamp.Ticks.ToString());
    It output_should_contain_exception = () => subject.ShouldContain(exception.Message);
    It output_should_contain_context = () => subject.ShouldContain("the Contextdata");

    static LogManager manager;
    static System.IO.StringWriter writer;
    static string subject;
    static Instant timestamp;
    static Exception exception;
  }

  public class When_configuring_filter_with_API
  {
    Establish context = () =>
        {
          writer = new System.IO.StringWriter(new StringBuilder());
          manager = LogaryFactory.New("Logary Specs",
                  with => with.Target<TextWriter.Builder>(
                      "sample string writer",
                      t => t.Target.WriteTo(writer, writer)
                            .AcceptIf(line => !line.name.ToString().Contains("When_configuring_filter_with_API"))))
                  .Result;
        };

    Cleanup cleanup = () =>
        {
          manager.Dispose();
          writer.Dispose();
        };

    Because reason = () =>
        {
          manager.GetLogger("Logary.Specs.When_configuring_filter_with_API")
                  .LogEvent(LogLevel.Warn, "the situation is dire", new { error = "oh-noes" });
          manager.FlushPending(Duration.FromSeconds(8L)).Wait();
          subject = writer.ToString();
        };

    It output_should_be_empty = () => subject.ShouldEqual("");

    static LogManager manager;
    static System.IO.StringWriter writer;
    static string subject;
  }
}

