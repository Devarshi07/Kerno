output "eks_cluster_role_arn" {
  value = aws_iam_role.eks_cluster.arn
}

output "eks_node_role_arn" {
  value = aws_iam_role.eks_node.arn
}

output "order_service_role_arn" {
  value = aws_iam_role.order_service.arn
}

output "notification_service_role_arn" {
  value = aws_iam_role.notification_service.arn
}
