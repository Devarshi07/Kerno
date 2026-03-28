output "orders_db_endpoint" {
  value = aws_db_instance.orders.endpoint
}

output "users_db_endpoint" {
  value = aws_db_instance.users.endpoint
}

output "keyspaces_endpoint" {
  value = "cassandra.${data.aws_region.current.name}.amazonaws.com:9142"
}

data "aws_region" "current" {}
