terraform {
  required_version = ">= 1.5.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  # In production, use S3 backend for remote state
  # backend "s3" {
  #   bucket         = "nexusgrid-terraform-state"
  #   key            = "terraform.tfstate"
  #   region         = "us-east-1"
  #   dynamodb_table = "nexusgrid-terraform-locks"
  #   encrypt        = true
  # }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = merge(var.tags, {
      Project     = var.project_name
      Environment = var.environment
      ManagedBy   = "terraform"
    })
  }
}

locals {
  name_prefix = "${var.project_name}-${var.environment}"
}

# ──────────────────────────────────────────────
# Networking — VPC, subnets, security groups
# ──────────────────────────────────────────────
module "networking" {
  source = "./modules/networking"

  name_prefix        = local.name_prefix
  vpc_cidr           = var.vpc_cidr
  availability_zones = var.availability_zones
}

# ──────────────────────────────────────────────
# IAM — Least-privilege roles per service
# ──────────────────────────────────────────────
module "iam" {
  source = "./modules/iam"

  name_prefix = local.name_prefix
}

# ──────────────────────────────────────────────
# Compute — EKS cluster for running services
# ──────────────────────────────────────────────
module "compute" {
  source = "./modules/compute"

  name_prefix        = local.name_prefix
  vpc_id             = module.networking.vpc_id
  private_subnet_ids = module.networking.private_subnet_ids
  node_instance_type = var.eks_node_instance_type
  desired_capacity   = var.eks_desired_capacity
  min_size           = var.eks_min_size
  max_size           = var.eks_max_size
  cluster_role_arn   = module.iam.eks_cluster_role_arn
  node_role_arn      = module.iam.eks_node_role_arn
}

# ──────────────────────────────────────────────
# Database — RDS PostgreSQL + Keyspaces (Cassandra)
# ──────────────────────────────────────────────
module "database" {
  source = "./modules/database"

  name_prefix        = local.name_prefix
  vpc_id             = module.networking.vpc_id
  private_subnet_ids = module.networking.private_subnet_ids
  db_security_group_id = module.networking.db_security_group_id
  instance_class     = var.rds_instance_class
  allocated_storage  = var.rds_allocated_storage
  db_password        = var.db_password
}

# ──────────────────────────────────────────────
# Cache — ElastiCache Redis
# ──────────────────────────────────────────────
module "cache" {
  source = "./modules/cache"

  name_prefix          = local.name_prefix
  private_subnet_ids   = module.networking.private_subnet_ids
  cache_security_group_id = module.networking.cache_security_group_id
  node_type            = var.redis_node_type
}
