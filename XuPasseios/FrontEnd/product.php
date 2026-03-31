<?php
session_start();
$ch = curl_init();
$item = 0;

$temp = explode("/",$_SERVER['REQUEST_URI']);
$productId = (end($temp));

$api_url = "https://localhost:7000/api/products/" . $productId;

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
?>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>E-Commerce</title>
    <link rel="stylesheet" href="/public/styles/product.css">
    <base href="http://localhost:3000"/>
</head>
<body>
    <?php require_once 'header.php'; ?>

    <main class="page__main">
        <div class="page__main__productdetails">
            <?php
                echo '<form action="cart.php" method="POST">
                        <div class="ProductDetails__Container">
                            <div class="ProductDetails__Image">
                                <figure>
                                    <img height="320px" width="320px" src="https://placehold.co/320x320" alt="Product Figure">
                                </figure>
                            </div>
                            <div class="ProducDetails__Description">
                                <div>
                                    <h2 class="main__product__h2">'. $data['productName'] .'</h2>
                                    <p class="main__product__description">'. $data['productDescription'] .'</p>
                                </div>
                                <div class="ProductDetails__Actions__Caracteristics">
                                    <p>
                                        <ul>
                                            <li>SKU: '. $data['sku'].'</li>
                                            <li>Weight:'. $data['productWeight'].'<br/></li>
                                            <li>A x L x P: <var>'.$data['productHeight'].'</var>&nbsp;x&nbsp;<var>'.$data['productWidth'].'</var>&nbsp;x&nbsp;<var>'.$data['productDepth'].'</var></li>
                                        </ul>
                                    </p>
                                </div>
                                <div>
                                    <a href="index.php"><button class="ProducDetails__Button__Return">Voltar</button></a>
                                </div>
                            </div>
                            <div class="ProductDetails__Actions">
                                    <div class="ProductDetails__Actions__Price">
                                        <p>Preço: R$&nbsp;'.$data['price'].'</p>
                                    </div>
                                    <div class="ProductDetails__Actions__CEP">
                                        <span>CEP:</span>
                                        <input type="text" maxlength="9"></input>
                                    </div>
                                    <!--<form action="cart.php" method="POST">-->
                                        <input type="hidden" name="productId" value="'. $productId .'"/>
                                        <input type="hidden" name="quantidade" value="1"/>
                                        <div class="ProductDetails__Actions__AddCart">
                                            <input type="submit" value="Comprar"></input>
                                        </div>
                                    <!--</form>-->
                            </div>
                        </div>
                    </form>';
            ?>
        </div>
    </main>
    <?php require_once 'footer.php'; ?>
</body>
</html>
