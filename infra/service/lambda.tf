resource "aws_lambda_function" "service" {
  function_name    = var.service_lambda_function_name
  filename         = var.service_lambda_filename
  source_code_hash = filebase64sha256(var.service_lambda_filename)
  runtime          = "provided.al2023"
  handler          = var.service_lambda_handler
  memory_size      = 512
  timeout          = 60

  layers = [
    "arn:aws:lambda:eu-central-1:580247275435:layer:LambdaInsightsExtension:53",
  ]

  role = aws_iam_role.function_iam_role.arn

}

resource "aws_cloudwatch_log_group" "cloudwatch_log_group" {
  name = "/aws/lambda/${aws_lambda_function.service.function_name}"

  retention_in_days = 30
}

resource "aws_iam_role" "function_iam_role" {
  name = var.service_lambda_function_name

  assume_role_policy = jsonencode({
    Version   = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Sid    = ""
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })
}

// Default Execution Policy
resource "aws_iam_role_policy_attachment" "execution" {
  role       = aws_iam_role.function_iam_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

// CloudWatch Insights Policy
resource "aws_iam_role_policy_attachment" "insights" {
  role       = aws_iam_role.function_iam_role.name
  policy_arn = "arn:aws:iam::aws:policy/CloudWatchLambdaInsightsExecutionRolePolicy"
}
