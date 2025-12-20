<?php
session_start();


if(isset($_SESSION["cart_items"]))
{
    $cartItems = $_SESSION["cart_items"];
    $quantity = $_POST['quantidade'];

    $_SESSION["checkoutMessage"] = $cartItems;
}
/******************************** */
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
        <form action="processCheckout.php" method="POST">
            <div class="checkout__main__body">
                <span>Total: R$</span>
                    <?php
                $total = 0;
                foreach ($cartItems as $key => $value) {
                    $total += $value["quantity"] * $value["price"];
                }
                echo '<span>' . $total . '</span>';
                ?>
            </div>
            <div>
                <button class="checkout__main__button" type="submit">Finalizar Compra</button>
            </div>
        </form>
    </main>
    <?php require_once 'footer.php'; ?>
</body>
</html>
