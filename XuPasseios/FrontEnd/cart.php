<?php
session_start();

$addsItemsToCard = $_SERVER["REQUEST_METHOD"] == "POST";
$justListItemFromCart = $_SERVER["REQUEST_METHOD"] == "GET";

$pageRefreshed = false;

// Check if a previous request exists in the session
if (isset($_SESSION['LAST_REQUEST'])
    // Compare the current request URI with the last one
    && $_SERVER['REQUEST_URI'] === $_SESSION['LAST_REQUEST']['REQUEST_URI'])
    {
        // Further check the HTTP_REFERER if available
        if (isset($_SERVER['HTTP_REFERER'])) {
            $pageRefreshed = ($_SERVER['HTTP_REFERER'] === $_SESSION['LAST_REQUEST']['HTTP_REFERER']);
        } else {
            // If no referrer on both, assume refresh
            $pageRefreshed = ($_SESSION['LAST_REQUEST']['HTTP_REFERER'] === null);
        }
    }

// Update the session with the current request details
$_SESSION['LAST_REQUEST'] = [
    'REQUEST_URI' => $_SERVER['REQUEST_URI'],
    'HTTP_REFERER' => isset($_SERVER['HTTP_REFERER']) ? $_SERVER['HTTP_REFERER'] : null,
];

/*if ($pageRefreshed) {
    echo "Page was refreshed!";
} else {
    echo "Page was loaded for the first time or navigated to.";
}*/

if ($addsItemsToCard && !$pageRefreshed)
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
            echo
                '<table class="cart__main__table">
                <thead>
                    <tr class="cart__main__tableHeader">
                        <th></th>
                        <th>Nome</th>
                        <th>Preço</th>
                        <th>Quantidade</th>
                        <th>Remover</th>
                    </tr>
                </thead>';
                

                if (!empty($cartItems))
                {
                    foreach ($cartItems as $val)
                    {
                        echo
                            '<tbody>
                                <tr class="cart__cartitem__tablerow">
                                    <td><img heigth="40px" width="40px" src="https://placehold.co/40x40" alt="'.$val["productName"].'"></td>
                                    <td>'.$val["productName"].'</td>
                                    <td><span>R$</span> '.$val["price"].'</td>
                                    <td><input type="number" minValue="0" class="cart__cartitem__quantity"></input></td>
                                    <td><button class="cart__cartitem__removes" onclick="RemovesItem(\'' . trim($val["productId"]," \n\r\t\v\x00") .'\');" >Remover</button></td>
                                </tr>
                            </tbody>';
                    }
                    echo '<tfoot>
                            <tr>
                                <td colspan="2"></td>
                                <td>Total:</td>
                                <td>R$</td>
                                <td class="cart__cartitem__checkout">
                                    <form action="checkout.php" method="POST">
                                        <input type="submit" value="Checkout"></button>
                                    </form>
                                </td>
                            </tr>
                        </tfoot>';
                            
                }
                    echo '</table>';
            ?>
        </div>

    </main>
    <footer>
    </footer>
</body>
<script >

    function RemovesItem(productId){
        let product = JSON.parse(`<?php echo json_encode($_SESSION["cart_items"]); ?>`).filter(item => item.productId === productId);
        
        var xhr = new XMLHttpRequest();
        xhr.open("POST", "removeItemFromCart.php", true);
        xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        
        xhr.onload = function(){
            if(xhr.status === 200){
                console.log(`${xhr.responseText}`);
                window.location.href= "http://localhost:3000/redirectToCart.php";
            }
            else{
                console.warn("Error during the removal of the product");
            }
        };
        console.log()
        xhr.send(JSON.stringify(product));
    }
</script>
</html>
