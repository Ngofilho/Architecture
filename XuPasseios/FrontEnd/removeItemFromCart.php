<?php
session_start();

$json_data = file_get_contents('php://input');

$data = json_decode($json_data, true);

if($data)
{
    foreach ($_SESSION["cart_items"] as $key => $value) {

        if ($value["productId"] === $data[0]["productId"]){
            unset($_SESSION["cart_items"][$key]);
            print_r($_SESSION["cart_items"]);
            $_SESSION["cart_items"] = array_values($_SESSION["cart_items"]);
            break;
        }
    }
}

//header("Refresh: 5; url=http://localhost:3000/cart.php");
header("Location" . "http://localhost:3000/redirectToCart.php");

?>
