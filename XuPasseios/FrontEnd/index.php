<?php
session_start();
$ch = curl_init();
$item = 0;

$api_url = "https://localhost:7000/api/products";

curl_setopt($ch, CURLOPT_SSL_VERIFYPEER , false);
curl_setopt($ch, CURLOPT_SSL_VERIFYHOST , false);


curl_setopt($ch, CURLOPT_URL, $api_url);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

// $htmlWebStorage = "<script>document.</script>";

$response = curl_exec($ch);

if(curl_errno($ch)){
    //echo 'cURL error: '. curl_error($ch);
    //echo '<script>console.error(\'' . curl_error ($ch) . ' \'); </script>';
    $response = "{\"value\":[{\"product\":{\"productId\":\"102b566b-ba1f-404c-b2df-e2cde39ade09\",\"productName\":\"Smile\",\"productDescription\":\"Happinesswhenyoulaughaboutsomesillything\",\"productWeight\":\"1ton\",\"productHeight\":1.0,\"productWidth\":1.0,\"productDepth\":1.0,\"sku\":1.0,\"price\":1.0,\"dateInclusion\":\"2025-11-04T00:00:00\",\"productActive\":false},\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":\"https://localhost:7000/api/products?productId=102b566b-ba1f-404c-b2df-e2cde39ade09\",\"rel\":\"create_product\",\"method\":\"POST\"}]}],\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":null,\"rel\":\"nextPage\",\"method\":\"GET\"}]}";
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
    <title>Agencia Xu Passeios</title>
    <link rel="stylesheet" href=".\public\styles\index.css">
    <base href="http://localhost:3000"/>    
</head>
<body>
    <?php require_once 'header.php'; ?>

    <main class="page__main">
        <div class="page__main__product">
            <?php
            foreach($data['value'] as $product)
            {                
                $item = $product['product'];
                echo '<div id="ProductCard">
                        <a id="ProductCardLink" href="product.php/'. $product['product']['productId'] .'">
                            <figure>
                                <img height="80px" width="80px" src="https://placehold.co/80x80" alt="Product Figure">
                            </figure>
                            <h2 class="main__product__h2">'. $product['product']['productName'] .'</h2>
                        </a>
                    </div>';
            }
            ?>
        </div>

    </main>
    
    <?php require_once 'footer.php'; ?>
    <script>
        <?php
        if(curl_errno($ch))  {
            echo 'console.error("Erro de comunicao. Codigo Erro:  ' . curl_errno($ch) . ' ; Mensagem do Erro: '. curl_error($ch) . '; ");';
        }
        ?>        
        if (typeof(Storage) !== undefined && sessionStorage.getItem("products") === null) {
            sessionStorage.setItem("products", "{\"value\":[{\"product\":{\"productId\":\"102b566b-ba1f-404c-b2df-e2cde39ade09\",\"productName\":\"Smile\",\"productDescription\":\"Happinesswhenyoulaughaboutsomesillything\",\"productWeight\":\"1ton\",\"productHeight\":1.0,\"productWidth\":1.0,\"productDepth\":1.0,\"sku\":1.0,\"price\":1.0,\"dateInclusion\":\"2025-11-04T00:00:00\",\"productActive\":false},\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":\"https://localhost:7000/api/products?productId=102b566b-ba1f-404c-b2df-e2cde39ade09\",\"rel\":\"create_product\",\"method\":\"POST\"}]},{\"product\":{\"productId\":\"164a2e03-bd51-4e0f-9e53-b7c181b00104\",\"productName\":\"Baffledmoment2\",\"productDescription\":\"Baffledmomentsoflifewhenyoudon'tknowwhattosay\",\"productWeight\":\"1ton\",\"productHeight\":1.0,\"productWidth\":1.0,\"productDepth\":1.0,\"sku\":1.0,\"price\":1.0,\"dateInclusion\":\"2025-11-05T00:00:00\",\"productActive\":false},\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":\"https://localhost:7000/api/products?productId=164a2e03-bd51-4e0f-9e53-b7c181b00104\",\"rel\":\"create_product\",\"method\":\"POST\"}]},{\"product\":{\"productId\":\"17f181fa-d2c7-4a1a-9848-163958e8b771\",\"productName\":\"Memories3\",\"productDescription\":\"Agoodmemorythathasbeenlost\",\"productWeight\":\"1ton\",\"productHeight\":1.0,\"productWidth\":1.0,\"productDepth\":1.0,\"sku\":1.0,\"price\":1.0,\"dateInclusion\":\"2025-11-08T00:00:00\",\"productActive\":false},\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":\"https://localhost:7000/api/products?productId=17f181fa-d2c7-4a1a-9848-163958e8b771\",\"rel\":\"create_product\",\"method\":\"POST\"}]},{\"product\":{\"productId\":\"20c1bfd6-1241-4a33-befd-d6392e23f5d6\",\"productName\":\"GoodFeeling3\",\"productDescription\":\"Thatfeelingonceexperiencedbutyoucan'tfinditanywhereatall\",\"productWeight\":\"1ton\",\"productHeight\":1.0,\"productWidth\":1.0,\"productDepth\":1.0,\"sku\":1.0,\"price\":1.0,\"dateInclusion\":\"2025-11-02T00:00:00\",\"productActive\":false},\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":\"https://localhost:7000/api/products?productId=20c1bfd6-1241-4a33-befd-d6392e23f5d6\",\"rel\":\"create_product\",\"method\":\"POST\"}]},{\"product\":{\"productId\":\"2902b665-1190-4c70-9915-b9c2d7680450\",\"productName\":\"NightSleep\",\"productDescription\":\"Thefirstnightthatyou'vespentawakewithoutanyspecialreasonandyouregretitdoingso\",\"productWeight\":\"1ton\",\"productHeight\":1.0,\"productWidth\":1.0,\"productDepth\":1.0,\"sku\":1.0,\"price\":1.0,\"dateInclusion\":\"2025-11-03T00:00:00\",\"productActive\":false},\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":\"https://localhost:7000/api/products?productId=2902b665-1190-4c70-9915-b9c2d7680450\",\"rel\":\"create_product\",\"method\":\"POST\"}]},{\"product\":{\"productId\":\"2aadd2df-7caf-45ab-9355-7f6332985a87\",\"productName\":\"Childhood\",\"productDescription\":\"Asmallpieceofchildhoodwhenthereweren'tanyworried\",\"productWeight\":\"1ton\",\"productHeight\":1.0,\"productWidth\":1.0,\"productDepth\":1.0,\"sku\":1.0,\"price\":1.0,\"dateInclusion\":\"2025-11-06T00:00:00\",\"productActive\":false},\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":\"https://localhost:7000/api/products?productId=2aadd2df-7caf-45ab-9355-7f6332985a87\",\"rel\":\"create_product\",\"method\":\"POST\"}]},{\"product\":{\"productId\":\"2ee49fe3-edf2-4f91-8409-3eb25ce6ca51\",\"productName\":\"UnicornHorn\",\"productDescription\":\"Magicartifactthatgrantmanypowertoitscarrier\",\"productWeight\":\"1ton\",\"productHeight\":1.0,\"productWidth\":1.0,\"productDepth\":1.0,\"sku\":1.0,\"price\":1.0,\"dateInclusion\":\"2025-11-07T00:00:00\",\"productActive\":false},\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":\"https://localhost:7000/api/products?productId=2ee49fe3-edf2-4f91-8409-3eb25ce6ca51\",\"rel\":\"create_product\",\"method\":\"POST\"}]},{\"product\":{\"productId\":\"34489196-0766-48bb-9918-f36e6ac81da8\",\"productName\":\"LostHope2\",\"productDescription\":\"Thatfeelingofhopeyou'vemissedduringyoulongwalkuntilthismomentinyourlife\",\"productWeight\":\"1ton\",\"productHeight\":1.0,\"productWidth\":1.0,\"productDepth\":1.0,\"sku\":1.0,\"price\":1.0,\"dateInclusion\":\"2025-11-01T00:00:00\",\"productActive\":false},\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":\"https://localhost:7000/api/products?productId=34489196-0766-48bb-9918-f36e6ac81da8\",\"rel\":\"create_product\",\"method\":\"POST\"}]},{\"product\":{\"productId\":\"375f3d27-caec-4416-a4e6-6d9e5685815e\",\"productName\":\"LostHope3\",\"productDescription\":\"Thatfeelingofhopeyou'vemissedduringyoulongwalkuntilthismomentinyourlife\",\"productWeight\":\"1ton\",\"productHeight\":1.0,\"productWidth\":1.0,\"productDepth\":1.0,\"sku\":1.0,\"price\":1.0,\"dateInclusion\":\"2025-11-01T00:00:00\",\"productActive\":false},\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":\"https://localhost:7000/api/products?productId=375f3d27-caec-4416-a4e6-6d9e5685815e\",\"rel\":\"create_product\",\"method\":\"POST\"}]},{\"product\":{\"productId\":\"42ed050d-37ec-4c1f-9bff-84990ac3adda\",\"productName\":\"NightSleep2\",\"productDescription\":\"Thefirstnightthatyou'vespentawakewithoutanyspecialreasonandyouregretitdoingso\",\"productWeight\":\"1ton\",\"productHeight\":1.0,\"productWidth\":1.0,\"productDepth\":1.0,\"sku\":1.0,\"price\":1.0,\"dateInclusion\":\"2025-11-03T00:00:00\",\"productActive\":false},\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":\"https://localhost:7000/api/products?productId=42ed050d-37ec-4c1f-9bff-84990ac3adda\",\"rel\":\"create_product\",\"method\":\"POST\"}]}],\"links\":[{\"href\":null,\"rel\":\"self\",\"method\":\"GET\"},{\"href\":null,\"rel\":\"nextPage\",\"method\":\"GET\"}]}");
        }
    </script>
</body>
</html>
