require 'bundler/setup'

require 'albacore'
require 'albacore/tasks/versionizer'
require 'albacore/ext/teamcity'

Albacore::Tasks::Versionizer.new :versioning

desc 'Perform fast build (warn: doesn\'t d/l deps)'
build :quick_build do |b|
  b.sln = 'src/Logary.sln'
end

desc 'restore all nugets as per the packages.config files'
nugets_restore :restore do |p|
  p.out = 'src/packages'
  p.exe = 'buildsupport/NuGet.exe'
end

desc 'create assembly infos'
asmver_files :assembly_info => :versioning do |a|
  a.files = FileList['**/*proj'] # optional, will find all projects recursively by default

  # attributes are required:
  a.attributes assembly_description: 'Logary is a high performance, multi-target logging, metric and health-check library for mono and .Net.',
               assembly_configuration: 'RELEASE',
               assembly_company: 'Intelliplan International AB',
               assembly_copyright: "(c) #{Time.now.year} by Henrik Feldt",
               assembly_version: ENV['LONG_VERSION'],
               assembly_file_version: ENV['LONG_VERSION'],
               assembly_informational_version: ENV['BUILD_VERSION']
end

desc 'perform full build'
build :build => [:versioning, :assembly_info, :restore] do |b|
  b.sln = 'src/Logary.sln'
end

directory 'build/pkg'

desc 'package nugets - finds all projects and package them'
nugets_pack :create_nugets => ['build/pkg', :versioning, :build] do |p|
  p.files   = FileList['src/**/*.{csproj,fsproj,nuspec}'].
    exclude('src/Fsharp.Actor/*.nuspec').
    exclude(/Fracture|Example|Tests|Spec|sample|packages/)
  p.out     = 'build/pkg'
  p.exe     = 'buildsupport/NuGet.exe'
  p.with_metadata do |m|
    m.description = 'Logary is a high performance, multi-target logging, metric and health-check library for mono and .Net.'
    m.authors = 'Henrik Feldt, Intelliplan International AB'
    m.version = ENV['NUGET_VERSION']
  end
end

task :default => :create_nugets

namespace :docs do
  task :pre_reqs do
    %w|FAKE FSharp.Formatting Microsoft.AspNet.Razor
       RazorEngine FSharp.Compiler.Service|.each do |dep|
      system 'buildsupport/NuGet.exe', %W|
        install #{dep} -OutputDirectory buildsupport -ExcludeVersion
      |, clr_command: true
    end
  end

  task :build => :pre_reqs do
    system 'buildsupport/FAKE/tools/Fake.exe', 'buildsupport/docs.fsx', clr_command: true
  end
end
