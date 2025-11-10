<?php
session_start();



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
    <header class="page__header">
        <div class="header__logo">
            <a href="index.php">
                <figure>
                    <img height="80px" width="80px" src="https://placehold.co/80x80" alt="E-Commerce Logo">
                </figure>
            </a>
        </div>
        <div class="page__header__search">
            <input type="text" class="page__header__text" name="search" id="searchBox">
        </div>
        <div>
            <a href="cart.php">
                <figure>
                    <img heigth="80px" width="80px" src="https://placehold.co/80x80" alt="Cart">
                </figure>
            </a>
        </div>
        <div>
            <a href="register.php">
                <figure>
                    <img heigth="80px" width="80px" src="https://placehold.co/80x80" alt="Register">
                </figure>
            </a>
        </div>
    </header>
    <main class="page__main">
    </main>
    <footer>
    </footer>
</body>
</html>
