# Subnet group for RDS (multi-AZ)
resource "aws_db_subnet_group" "main" {
  name       = "${var.name_prefix}-db-subnet-group"
  subnet_ids = var.private_subnet_ids

  tags = { Name = "${var.name_prefix}-db-subnet-group" }
}

# RDS PostgreSQL — Orders database
resource "aws_db_instance" "orders" {
  identifier     = "${var.name_prefix}-orders-db"
  engine         = "postgres"
  engine_version = "16.3"
  instance_class = var.instance_class

  allocated_storage     = var.allocated_storage
  max_allocated_storage = var.allocated_storage * 2

  db_name  = "nexusgrid_orders"
  username = "postgres"
  password = var.db_password

  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [var.db_security_group_id]

  multi_az            = false
  publicly_accessible = false
  skip_final_snapshot = true
  storage_encrypted   = true

  backup_retention_period = 7
  backup_window           = "03:00-04:00"
  maintenance_window      = "Mon:04:00-Mon:05:00"

  tags = { Name = "${var.name_prefix}-orders-db" }
}

# RDS PostgreSQL — Users database (separate per microservice ownership)
resource "aws_db_instance" "users" {
  identifier     = "${var.name_prefix}-users-db"
  engine         = "postgres"
  engine_version = "16.3"
  instance_class = var.instance_class

  allocated_storage     = var.allocated_storage
  max_allocated_storage = var.allocated_storage * 2

  db_name  = "nexusgrid_users"
  username = "postgres"
  password = var.db_password

  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [var.db_security_group_id]

  multi_az            = false
  publicly_accessible = false
  skip_final_snapshot = true
  storage_encrypted   = true

  backup_retention_period = 7
  backup_window           = "03:00-04:00"
  maintenance_window      = "Mon:04:00-Mon:05:00"

  tags = { Name = "${var.name_prefix}-users-db" }
}

# Amazon Keyspaces (managed Cassandra)
resource "aws_keyspaces_keyspace" "main" {
  name = "nexusgrid"
}

resource "aws_keyspaces_table" "notifications_by_user" {
  keyspace_name = aws_keyspaces_keyspace.main.name
  table_name    = "notifications_by_user"

  schema_definition {
    column { name = "user_id";    type = "uuid" }
    column { name = "created_at"; type = "timestamp" }
    column { name = "id";         type = "uuid" }
    column { name = "type";       type = "text" }
    column { name = "status";     type = "text" }
    column { name = "title";      type = "text" }
    column { name = "message";    type = "text" }

    partition_key { name = "user_id" }
    clustering_key { name = "created_at"; order_by = "DESC" }
    clustering_key { name = "id";         order_by = "ASC" }
  }
}

resource "aws_keyspaces_table" "audit_events_by_tenant" {
  keyspace_name = aws_keyspaces_keyspace.main.name
  table_name    = "audit_events_by_tenant"

  schema_definition {
    column { name = "tenant_id";     type = "text" }
    column { name = "event_date";    type = "text" }
    column { name = "event_time";    type = "timestamp" }
    column { name = "id";            type = "uuid" }
    column { name = "event_type";    type = "text" }
    column { name = "actor_id";      type = "text" }
    column { name = "resource_type"; type = "text" }
    column { name = "resource_id";   type = "text" }
    column { name = "description";   type = "text" }

    partition_key  { name = "tenant_id" }
    partition_key  { name = "event_date" }
    clustering_key { name = "event_time"; order_by = "DESC" }
    clustering_key { name = "id";         order_by = "ASC" }
  }
}
