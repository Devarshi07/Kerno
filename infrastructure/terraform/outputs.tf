output "vpc_id" {
  description = "VPC ID"
  value       = module.networking.vpc_id
}

output "eks_cluster_endpoint" {
  description = "EKS cluster API endpoint"
  value       = module.compute.cluster_endpoint
}

output "eks_cluster_name" {
  description = "EKS cluster name"
  value       = module.compute.cluster_name
}

output "orders_db_endpoint" {
  description = "RDS endpoint for Orders database"
  value       = module.database.orders_db_endpoint
}

output "users_db_endpoint" {
  description = "RDS endpoint for Users database"
  value       = module.database.users_db_endpoint
}

output "keyspaces_endpoint" {
  description = "Amazon Keyspaces endpoint"
  value       = module.database.keyspaces_endpoint
}

output "redis_endpoint" {
  description = "ElastiCache Redis endpoint"
  value       = module.cache.redis_endpoint
}
