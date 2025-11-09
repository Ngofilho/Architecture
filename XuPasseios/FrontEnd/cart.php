<?php
session_start();

$addsItemsToCard = $_SERVER["REQUEST_METHOD"] == "POST";

if ($addsItemsToCard)
{
    $productId = $_POST['productId'];

    $ch = curl_init();
    $item = 0;

    $api_url = "https://localhost:7140/api/products/" . $productId;

    curl_setopt($ch, CURLOPT_SSL_VERIFYPEER , false);
    curl_setopt($ch, CURLOPT_SSL_VERIFYHOST , false);

    curl_setopt($ch, CURLOPT_URL, $api_url);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

    $response = curl_exec($ch);

    if(curl_errno($ch)){
        echo 'cURL error: '. curl_error($ch);
    }

    curl_close($ch);

    $data = json_decode($response, true);


    if(!$data)
    {
        echo 'Failed to decode JSON response.';
    }

    if (!isset($_SESSION["cart_items"]))
    {
        $_SESSION["cart_items"] = array();
    }

    $_SESSION["cart_items"][] = $data;
}

if(isset($_SESSION["cart_items"]))
{
    $cartItems = $_SESSION["cart_items"];
}

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
        <div class="cart__main__content">
            <?php
                print_r($cartItems);
                if (!empty($cartItems))
                {
                    echo
                    '<table class="cart__main__table">
                        <tr class="cart__main__tableHeader">
                            <th></th>
                            <th>Nome</th>
                            <th>Preço</th>
                            <th>Quantidade</th>
                            <th>Remover</th>
                        </tr>';
                        
                        foreach ($cartItems as $val)
                        {
                            echo
                                '<tr>
                                    <td><img heigth="40px" width="40px" src="https://placehold.co/40x40" alt="'.$val["productName"].'"></td>
                                    <td style="font-size:2.5rem;">'.$val["productName"].'</td>
                                    <td style="font-size:2.5rem;"><span>R$</span> '.$val["price"].'</td>
                                    <td><input type="number" minValue="0"></input></td>
                                    <td><button onclick="RemovesItem("' . trim($val["productId"]," \n\r\t\v\x00") . '");" value="Remover">Remover</button></td>
                                </tr>';
                        }
                        echo '</table>';
                }
            ?>
        </div>

    </main>
    
    <footer>
         <?php
         if ($addsItemsToCard)
         {
            print_r($data);
         }
         else {
            print_r('There aren\'t items to show');
         }
         ?>
    </footer>
</body>
</html>
