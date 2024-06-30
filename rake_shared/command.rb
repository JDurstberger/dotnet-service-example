require 'open3'

module Command
  class << self
    def run(command, environment = {}, config = {})
      Open3.popen2e(environment.transform_keys(&:to_s), command) do |stdin, stdout_stderr, wait_thread|
        Thread.new do
          stdout_stderr.each {|l| puts l }
        end

        stdin.close
        exit_status = wait_thread.value
        stdout_stderr.close

        unless exit_status.success?
          raise("Failed with exit code: #{exit_status.exitstatus}")
        end
      end
    end
  end
end