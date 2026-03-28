environment        = "production"
aws_region         = "us-east-1"
vpc_cidr           = "10.1.0.0/16"
availability_zones = ["us-east-1a", "us-east-1b", "us-east-1c"]

# Production-grade instances
eks_node_instance_type = "t3.large"
eks_desired_capacity   = 3
eks_min_size           = 2
eks_max_size           = 6

rds_instance_class    = "db.t3.medium"
rds_allocated_storage = 50

redis_node_type = "cache.t3.small"

tags = {
  CostCenter = "engineering"
  Team       = "platform"
}
