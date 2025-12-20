<?php
session_start();

unset($_SESSION['cart_items']);

?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>E-Commerce</title>
    <link rel="stylesheet" href="/public/styles/cart.css">
    <base href="http://localhost:3000"/>
</head>
<body>
    <?php require_once 'header.php'; ?>
    <main class="page__main">
    </main>
    <?php require_once 'footer.php'; ?>
</body>
</html>
