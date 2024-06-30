require 'confidante'
require 'rake_terraform'

require './rake_shared/command'

RakeTerraform.define_installation_tasks(
  path: File.join(Dir.pwd, 'vendor', 'terraform'),
  version: '1.8.1')

project_configuration = Confidante.configuration
app_project_dir = project_configuration.app_project_dir

namespace :app do
  task :build do
    puts "Building..."
    Command.run("dotnet build")
  end

  task :package do
    puts "Packaging..."
    Dir.chdir(app_project_dir) do
      Command.run("dotnet lambda package")
    end
  end

end

namespace :bootstrap do
  RakeTerraform.define_command_tasks(
    configuration_name: 'bootstrap infrastructure',
    argument_names: [:deployment_identifier]) do |t, args|
    configuration = Confidante.configuration.for_scope(args.to_h.merge(role: 'bootstrap'))

    deployment_identifier = args.deployment_identifier
    vars = configuration.vars
    t.source_directory = "/infra/bootstrap"
    t.work_directory = 'build'

    t.state_file =
      File.join(Dir.pwd, "/state/bootstrap/#{deployment_identifier}.tfstate")
    t.vars = vars
  end
end

namespace :service do
  infra_dir = "service"

  task :lint do
    sh "tflint --init"
    sh "tflint --config=$(pwd)/.tflint.hcl --chdir=/infra/#{infra_dir}"
  end

  RakeTerraform.define_command_tasks(
    configuration_name: 'service account setup',
    argument_names: [:deployment_identifier]) do |t, args|
    configuration = Confidante.configuration.for_scope(args.to_h.merge(role: 'service'))

    t.source_directory = "infra/#{infra_dir}"
    t.work_directory = 'build'

    t.backend_config = configuration.backend_config
    t.vars = configuration.vars
  end
end