environment        = "staging"
aws_region         = "us-east-1"
vpc_cidr           = "10.0.0.0/16"
availability_zones = ["us-east-1a", "us-east-1b"]

# Smaller instances for staging
eks_node_instance_type = "t3.medium"
eks_desired_capacity   = 2
eks_min_size           = 1
eks_max_size           = 3

rds_instance_class    = "db.t3.micro"
rds_allocated_storage = 20

redis_node_type = "cache.t3.micro"

tags = {
  CostCenter = "engineering"
  Team       = "platform"
}
